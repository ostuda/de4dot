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
using System.Reflection;
using AssemblyData.methodsrewriter;

namespace AssemblyData {
	class EmuStringDecrypter : IStringDecrypter {
		List<DecryptInfo> decryptInfos = new List<DecryptInfo>();
		MethodsRewriter methodsRewriter = new MethodsRewriter();

		class DecryptInfo {
			public MethodInfo method;
			public RewrittenMethod decryptString;

			public DecryptInfo(MethodInfo method) {
				this.method = method;
			}
		}

		public int defineStringDecrypter(MethodInfo method) {
			decryptInfos.Add(new DecryptInfo(method));
			return decryptInfos.Count - 1;
		}

		public object[] decryptStrings(int stringDecrypterMethod, object[] args) {
			var decryptInfo = decryptInfos[stringDecrypterMethod];
			if (decryptInfo.decryptString == null)
				decryptInfo.decryptString = createDecryptString(decryptInfo.method);

			var result = new object[args.Length];
			for (int i = 0; i < args.Length; i++)
				result[i] = decryptInfo.decryptString((object[])args[i]);
			return result;
		}

		RewrittenMethod createDecryptString(MethodInfo method) {
			methodsRewriter.createMethod(method);
			return methodsRewriter.createDelegate(method);
		}
	}
}
