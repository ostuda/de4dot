﻿/*
    Copyright (C) 2011 de4dot@gmail.com

    This file is part of de4dot.

    de4dot is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    de4dot is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with de4dot.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace de4dot.blocks {
	// This class makes sure that each block that is entered with a non-empty stack has at
	// least one of its source blocks sorted before itself. This is to make sure peverify
	// doesn't complain AND also to make sure Mono.Cecil sets the correct maxstack.
	class ForwardScanOrder {
		ScopeBlock scopeBlock;
		IList<BaseBlock> sorted;
		Dictionary<BaseBlock, BlockInfo> blockInfos = new Dictionary<BaseBlock, BlockInfo>();
		Dictionary<BaseBlock, bool> inNewList = new Dictionary<BaseBlock, bool>();
		List<BaseBlock> newList;

		class BlockInfo {
			BaseBlock baseBlock;
			public int stackStart = 0;
			public int stackEnd = 0;

			public BlockInfo(BaseBlock baseBlock, int stackStart) {
				this.baseBlock = baseBlock;
				this.stackStart = stackStart;
			}

			public void calculateStackUsage() {
				Block block = baseBlock as Block;
				if (block == null) {
					stackEnd = stackStart;
					return;
				}

				int stack = stackStart;
				foreach (var instr in block.Instructions)
					DotNetUtils.updateStack(instr.Instruction, ref stack);
				stackEnd = stack;
			}
		}

		public ForwardScanOrder(ScopeBlock scopeBlock, IList<BaseBlock> sorted) {
			this.scopeBlock = scopeBlock;
			this.sorted = sorted;
		}

		public List<BaseBlock> fix() {
			createBlockInfos();
			createNewList();
			return newList;
		}

		void createBlockInfos() {
			int firstBlockStackStart = scopeBlock is TryHandlerBlock ? 1 : 0;
			foreach (var bb in getStartBlocks()) {
				int stackStart = ReferenceEquals(bb, sorted[0]) ? firstBlockStackStart : 0;
				scanBaseBlock(bb, stackStart);
			}

			// One reason for this to fail is if there are still dead blocks left. Could also
			// be a bug in the code.
			if (blockInfos.Count != sorted.Count)
				throw new ApplicationException(string.Format("Didn't add all blocks: {0} vs {1}", blockInfos.Count, sorted.Count));
		}

		IEnumerable<BaseBlock> getStartBlocks() {
			if (sorted.Count > 0) {
				yield return sorted[0];
				foreach (var bb in sorted) {
					if (ReferenceEquals(bb, sorted[0]))
						continue;
					var block = bb as Block;
					if (block == null || block.Sources == null || isOneSourceInAnotherScopeBlock(block))
						yield return bb;
				}
			}
		}

		bool isOneSourceInAnotherScopeBlock(Block block) {
			foreach (var source in block.Sources) {
				if (!scopeBlock.isOurBlockBase(source))
					return true;
			}
			return false;
		}

		void scanBaseBlock(BaseBlock bb, int stackStart) {
			if (blockInfos.ContainsKey(bb) || !scopeBlock.isOurBlockBase(bb))
				return;

			var blockInfo = new BlockInfo(bb, stackStart);
			blockInfos[bb] = blockInfo;

			var block = bb as Block;
			if (block == null) {	// i.e., if try, filter, or handler block
				// It's not important to know the exact values, so we set them both to 0.
				// Compilers must make sure the stack is empty when entering a try block.
				blockInfo.stackStart = blockInfo.stackEnd = 0;
				return;
			}

			blockInfo.calculateStackUsage();

			foreach (var target in block.getTargets())
				scanBaseBlock(target, blockInfo.stackEnd);
		}

		void createNewList() {
			newList = new List<BaseBlock>(sorted.Count);
			foreach (var bb in sorted)
				addToNewList(bb);
			if (newList.Count != sorted.Count)
				throw new ApplicationException(string.Format("Too many/few blocks after sorting: {0} vs {1}", newList.Count, sorted.Count));
			if (newList.Count > 0 && !ReferenceEquals(newList[0], sorted[0]))
				throw new ApplicationException("Start block is not first block after sorting");
		}

		void addToNewList(BaseBlock bb) {
			if (inNewList.ContainsKey(bb) || !scopeBlock.isOurBlockBase(bb))
				return;
			inNewList[bb] = true;

			var blockInfo = blockInfos[bb];
			var block = bb as Block;
			if (blockInfo.stackStart == 0 || ReferenceEquals(bb, sorted[0]) ||
				block == null || block.Sources == null || isInNewList(block.Sources)) {
			}
			else {
				foreach (var source in block.Sources) {
					if (scopeBlock.isOurBlockBase(source)) {
						addToNewList(source);	// Make sure it's before this block
						break;
					}
				}
			}

			newList.Add(bb);
		}

		bool isInNewList(IEnumerable<Block> blocks) {
			foreach (var block in blocks) {
				if (inNewList.ContainsKey(block))
					return true;
			}
			return false;
		}
	}
}
