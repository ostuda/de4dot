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

using System.Collections.Generic;

namespace de4dot.deobfuscators {
	abstract class DeobfuscatorInfoBase : IDeobfuscatorInfo {
		string argPrefix;
		protected NameRegexOption validNameRegex;

		public DeobfuscatorInfoBase(string argPrefix, string nameRegex = null) {
			this.argPrefix = argPrefix;
			validNameRegex = new NameRegexOption(null, makeArgName("name"), "Valid name regex pattern", nameRegex ?? DeobfuscatorBase.DEFAULT_VALID_NAME_REGEX);
		}

		protected string makeArgName(string name) {
			return string.Format("{0}-{1}", argPrefix, name);
		}

		public abstract string Type { get; }
		public abstract IDeobfuscator createDeobfuscator();

		protected virtual IEnumerable<Option> getOptionsInternal() {
			return new List<Option>();
		}

		public IEnumerable<Option> getOptions() {
			var options = new List<Option>();
			options.Add(validNameRegex);
			options.AddRange(getOptionsInternal());
			return options;
		}
	}
}
