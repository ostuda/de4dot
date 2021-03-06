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
using Mono.Cecil;
using de4dot.blocks;

namespace AssemblyData.methodsrewriter {
	class TypeResolver {
		public Type type;
		Dictionary<TypeReferenceKey, TypeInstanceResolver> typeRefToInstance = new Dictionary<TypeReferenceKey, TypeInstanceResolver>();

		public TypeResolver(Type type) {
			this.type = type;
		}

		TypeInstanceResolver getTypeInstance(TypeReference typeReference) {
			var key = new TypeReferenceKey(typeReference);
			TypeInstanceResolver instance;
			if (!typeRefToInstance.TryGetValue(key, out instance))
				typeRefToInstance[key] = instance = new TypeInstanceResolver(type, typeReference);
			return instance;
		}

		public FieldInfo resolve(FieldReference fieldReference) {
			return getTypeInstance(fieldReference.DeclaringType).resolve(fieldReference);
		}

		public MethodBase resolve(MethodReference methodReference) {
			return getTypeInstance(methodReference.DeclaringType).resolve(methodReference);
		}
	}
}
