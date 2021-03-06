/*
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
using Mono.Cecil.Cil;
using System.Collections.Generic;

namespace de4dot.blocks {
	public class Instr {
		Instruction instruction;

		public OpCode OpCode {
			get { return instruction.OpCode; }
		}

		public object Operand {
			get { return instruction.Operand; }
			set { instruction.Operand = value; }
		}

		public Instr(Instruction instruction) {
			this.instruction = instruction;
		}

		public Instruction Instruction {
			get { return instruction; }
		}

		// Returns the variable or null if it's not a ldloc/stloc instruction
		public static VariableDefinition getLocalVar(IList<VariableDefinition> locals, Instr instr) {
			switch (instr.OpCode.Code) {
			case Code.Ldloc:
			case Code.Ldloc_S:
			case Code.Stloc:
			case Code.Stloc_S:
				return (VariableDefinition)instr.Operand;

			case Code.Ldloc_0:
			case Code.Ldloc_1:
			case Code.Ldloc_2:
			case Code.Ldloc_3:
				return locals[instr.OpCode.Code - Code.Ldloc_0];

			case Code.Stloc_0:
			case Code.Stloc_1:
			case Code.Stloc_2:
			case Code.Stloc_3:
				return locals[instr.OpCode.Code - Code.Stloc_0];

			default:
				return null;
			}
		}

		static public bool isFallThrough(OpCode opCode) {
			switch (opCode.FlowControl) {
			case FlowControl.Call:
				return opCode != OpCodes.Jmp;
			case FlowControl.Cond_Branch:
			case FlowControl.Next:
				return true;
			default:
				return false;
			}
		}

		// Returns true if the instruction only pushes one value onto the stack and pops nothing
		public bool isSimpleLoad() {
			switch (OpCode.Code) {
			case Code.Ldarg:
			case Code.Ldarg_S:
			case Code.Ldarg_0:
			case Code.Ldarg_1:
			case Code.Ldarg_2:
			case Code.Ldarg_3:
			case Code.Ldarga:
			case Code.Ldarga_S:
			case Code.Ldc_I4:
			case Code.Ldc_I4_S:
			case Code.Ldc_I4_0:
			case Code.Ldc_I4_1:
			case Code.Ldc_I4_2:
			case Code.Ldc_I4_3:
			case Code.Ldc_I4_4:
			case Code.Ldc_I4_5:
			case Code.Ldc_I4_6:
			case Code.Ldc_I4_7:
			case Code.Ldc_I4_8:
			case Code.Ldc_I4_M1:
			case Code.Ldc_I8:
			case Code.Ldc_R4:
			case Code.Ldc_R8:
			case Code.Ldloc:
			case Code.Ldloc_S:
			case Code.Ldloc_0:
			case Code.Ldloc_1:
			case Code.Ldloc_2:
			case Code.Ldloc_3:
			case Code.Ldloca:
			case Code.Ldloca_S:
			case Code.Ldnull:
			case Code.Ldstr:
			case Code.Ldtoken:
				return true;
			default:
				return false;
			}
		}

		// Returns true if it's one of the ldc.i4 instructions
		public bool isLdcI4() {
			return DotNetUtils.isLdcI4(OpCode.Code);
		}

		public int getLdcI4Value() {
			return DotNetUtils.getLdcI4Value(instruction);
		}

		// Return true if it's one of the stloc instructions
		public bool isStloc() {
			return OpCode == OpCodes.Stloc ||
					OpCode == OpCodes.Stloc_0 ||
					OpCode == OpCodes.Stloc_1 ||
					OpCode == OpCodes.Stloc_2 ||
					OpCode == OpCodes.Stloc_3 ||
					OpCode == OpCodes.Stloc_S;
		}

		// Returns true if it's one of the ldloc instructions
		public bool isLdloc() {
			return OpCode == OpCodes.Ldloc ||
					OpCode == OpCodes.Ldloc_0 ||
					OpCode == OpCodes.Ldloc_1 ||
					OpCode == OpCodes.Ldloc_2 ||
					OpCode == OpCodes.Ldloc_3 ||
					OpCode == OpCodes.Ldloc_S;
		}

		public bool isNop() {
			return OpCode == OpCodes.Nop;
		}

		public bool isPop() {
			return OpCode == OpCodes.Pop;
		}

		// Returns true if it's a leave/leave.s
		public bool isLeave() {
			return OpCode == OpCodes.Leave || OpCode == OpCodes.Leave_S;
		}

		// Returns true if it's a br or br.s instruction
		public bool isBr() {
			return OpCode == OpCodes.Br || OpCode == OpCodes.Br_S;
		}

		// Returns true if it's a brfalse/brfalse.s instr
		public bool isBrfalse() {
			return OpCode == OpCodes.Brfalse || OpCode == OpCodes.Brfalse_S;
		}

		// Returns true if it's a brtrue/brtrue.s instr
		public bool isBrtrue() {
			return OpCode == OpCodes.Brtrue || OpCode == OpCodes.Brtrue_S;
		}

		public bool isConditionalBranch() {
			return DotNetUtils.isConditionalBranch(OpCode.Code);
		}

		public bool getFlippedBranchOpCode(out OpCode opcode) {
			switch (OpCode.Code) {
			case Code.Bge:		opcode = OpCodes.Blt; return true;
			case Code.Bge_S:	opcode = OpCodes.Blt_S; return true;
			case Code.Bge_Un:	opcode = OpCodes.Blt_Un; return true;
			case Code.Bge_Un_S: opcode = OpCodes.Blt_Un_S; return true;

			case Code.Blt:		opcode = OpCodes.Bge; return true;
			case Code.Blt_S:	opcode = OpCodes.Bge_S; return true;
			case Code.Blt_Un:	opcode = OpCodes.Bge_Un; return true;
			case Code.Blt_Un_S: opcode = OpCodes.Bge_Un_S; return true;

			case Code.Bgt:		opcode = OpCodes.Ble; return true;
			case Code.Bgt_S:	opcode = OpCodes.Ble_S; return true;
			case Code.Bgt_Un:	opcode = OpCodes.Ble_Un; return true;
			case Code.Bgt_Un_S: opcode = OpCodes.Ble_Un_S; return true;

			case Code.Ble:		opcode = OpCodes.Bgt; return true;
			case Code.Ble_S:	opcode = OpCodes.Bgt_S; return true;
			case Code.Ble_Un:	opcode = OpCodes.Bgt_Un; return true;
			case Code.Ble_Un_S: opcode = OpCodes.Bgt_Un_S; return true;

			case Code.Brfalse:	opcode = OpCodes.Brtrue; return true;
			case Code.Brfalse_S:opcode = OpCodes.Brtrue_S; return true;

			case Code.Brtrue:	opcode = OpCodes.Brfalse; return true;
			case Code.Brtrue_S: opcode = OpCodes.Brfalse_S; return true;

			// Can't flip beq and bne.un since it's object vs uint/float
			case Code.Beq:
			case Code.Beq_S:
			case Code.Bne_Un:
			case Code.Bne_Un_S:
			default:
				opcode = OpCodes.Nop;	// Whatever...
				return false;
			}
		}

		public void flipConditonalBranch() {
			OpCode opcode;
			if (!getFlippedBranchOpCode(out opcode))
				throw new ApplicationException("Can't flip conditional since it's not a supported conditional instruction");
			instruction.OpCode = opcode;
		}

		// Returns true if we can flip a conditional branch
		public bool canFlipConditionalBranch() {
			OpCode opcode;
			return getFlippedBranchOpCode(out opcode);
		}

		public void updateTargets(List<Instr> targets) {
			switch (OpCode.OperandType) {
			case OperandType.ShortInlineBrTarget:
			case OperandType.InlineBrTarget:
				if (targets.Count != 1)
					throw new ApplicationException("More than one target!");
				instruction.Operand = targets[0].Instruction;
				break;

			case OperandType.InlineSwitch:
				if (targets.Count == 0)
					throw new ApplicationException("No targets!");
				var switchTargets = new Instruction[targets.Count];
				for (var i = 0; i < targets.Count; i++)
					switchTargets[i] = targets[i].Instruction;
				instruction.Operand = switchTargets;
				break;

			default:
				if (targets.Count != 0)
					throw new ApplicationException("This instruction doesn't have any targets!");
				break;
			}
		}

		public override string ToString() {
			return instruction.ToString();
		}
	}
}
