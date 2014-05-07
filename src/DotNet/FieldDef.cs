/*
    Copyright (C) 2012-2014 de4dot@gmail.com

    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the
    "Software"), to deal in the Software without restriction, including
    without limitation the rights to use, copy, modify, merge, publish,
    distribute, sublicense, and/or sell copies of the Software, and to
    permit persons to whom the Software is furnished to do so, subject to
    the following conditions:

    The above copyright notice and this permission notice shall be
    included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
    MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
    SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

﻿using System;
using System.Threading;
using dnlib.Utils;
using dnlib.DotNet.MD;
using dnlib.PE;
using dnlib.Threading;

namespace dnlib.DotNet {
	/// <summary>
	/// A high-level representation of a row in the Field table
	/// </summary>
	public abstract class FieldDef : IHasConstant, IHasCustomAttribute, IHasFieldMarshal, IMemberForwarded, IField, ITokenOperand, IMemberDef {
		/// <summary>
		/// The row id in its table
		/// </summary>
		protected uint rid;

#if THREAD_SAFE
		/// <summary>
		/// The lock
		/// </summary>
		internal readonly Lock theLock = Lock.Create();
#endif

		/// <inheritdoc/>
		public MDToken MDToken {
			get { return new MDToken(Table.Field, rid); }
		}

		/// <inheritdoc/>
		public uint Rid {
			get { return rid; }
			set { rid = value; }
		}

		/// <inheritdoc/>
		public int HasConstantTag {
			get { return 0; }
		}

		/// <inheritdoc/>
		public int HasCustomAttributeTag {
			get { return 1; }
		}

		/// <inheritdoc/>
		public int HasFieldMarshalTag {
			get { return 0; }
		}

		/// <inheritdoc/>
		public int MemberForwardedTag {
			get { return 0; }
		}

		/// <summary>
		/// Gets all custom attributes
		/// </summary>
		public abstract CustomAttributeCollection CustomAttributes { get; }

		/// <summary>
		/// From column Field.Flags
		/// </summary>
		public FieldAttributes Attributes {
#if THREAD_SAFE
			get {
				theLock.EnterWriteLock();
				try {
					return Attributes_NoLock;
				} finally { theLock.ExitWriteLock(); }
			}
			set {
				theLock.EnterWriteLock();
				try {
					Attributes_NoLock = value;
				} finally { theLock.ExitWriteLock(); }
			}
#else
			get { return Attributes_NoLock; }
			set { Attributes_NoLock = value; }
#endif
		}

		/// <summary>
		/// From column Field.Flags
		/// </summary>
		protected abstract FieldAttributes Attributes_NoLock { get; set; }

		/// <summary>
		/// From column Field.Name
		/// </summary>
		public abstract UTF8String Name { get; set; }

		/// <summary>
		/// From column Field.Signature
		/// </summary>
		public abstract CallingConventionSig Signature { get; set; }

		/// <summary>
		/// Gets/sets the field layout offset
		/// </summary>
		public abstract uint? FieldOffset { get; set; }

		/// <inheritdoc/>
		public abstract MarshalType MarshalType { get; set; }

		/// <summary>
		/// Gets/sets the field RVA
		/// </summary>
		public abstract RVA RVA { get; set; }

		/// <summary>
		/// Gets/sets the initial value
		/// </summary>
		public abstract byte[] InitialValue { get; set; }

		/// <inheritdoc/>
		public abstract ImplMap ImplMap { get; set; }

		/// <inheritdoc/>
		public abstract Constant Constant { get; set; }

		/// <inheritdoc/>
		public bool HasCustomAttributes {
			get { return CustomAttributes.Count > 0; }
		}

		/// <inheritdoc/>
		public bool HasImplMap {
			get { return ImplMap != null; }
		}

		/// <summary>
		/// Gets/sets the declaring type (owner type)
		/// </summary>
		public TypeDef DeclaringType {
			get { return DeclaringType2; }
			set {
				var currentDeclaringType = DeclaringType2;
				if (currentDeclaringType == value)
					return;
				if (currentDeclaringType != null)
					currentDeclaringType.Fields.Remove(this);	// Will set DeclaringType2 = null
				if (value != null)
					value.Fields.Add(this);		// Will set DeclaringType2 = value
			}
		}

		/// <inheritdoc/>
		ITypeDefOrRef IMemberRef.DeclaringType {
			get { return DeclaringType; }
		}

		/// <summary>
		/// Called by <see cref="DeclaringType"/> and should normally not be called by any user
		/// code. Use <see cref="DeclaringType"/> instead. Only call this if you must set the
		/// declaring type without inserting it in the declaring type's method list.
		/// </summary>
		public TypeDef DeclaringType2 {
#if THREAD_SAFE
			get {
				theLock.EnterWriteLock(); try {
					return DeclaringType2_NoLock;
				}
				finally { theLock.ExitWriteLock(); }
			}
			set {
				theLock.EnterWriteLock(); try {
					DeclaringType2_NoLock = value;
				}
				finally { theLock.ExitWriteLock(); }
			}
#else
			get { return DeclaringType2_NoLock; }
			set { DeclaringType2_NoLock = value; }
#endif
		}

		/// <summary>
		/// No-lock version of <see cref="DeclaringType2"/>.
		/// </summary>
		internal abstract TypeDef DeclaringType2_NoLock { get; set; }

		/// <summary>
		/// Gets/sets the <see cref="FieldSig"/>
		/// </summary>
		public FieldSig FieldSig {
			get { return Signature as FieldSig; }
			set { Signature = value; }
		}

		/// <inheritdoc/>
		public ModuleDef Module {
			get {
				var dt = DeclaringType;
				return dt == null ? null : dt.Module;
			}
		}

		bool IMemberRef.IsType {
			get { return false; }
		}

		bool IMemberRef.IsMethod {
			get { return false; }
		}

		bool IMemberRef.IsField {
			get { return true; }
		}

		bool IMemberRef.IsTypeSpec {
			get { return false; }
		}

		bool IMemberRef.IsTypeRef {
			get { return false; }
		}

		bool IMemberRef.IsTypeDef {
			get { return false; }
		}

		bool IMemberRef.IsMethodSpec {
			get { return false; }
		}

		bool IMemberRef.IsMethodDef {
			get { return false; }
		}

		bool IMemberRef.IsMemberRef {
			get { return false; }
		}

		bool IMemberRef.IsFieldDef {
			get { return true; }
		}

		bool IMemberRef.IsPropertyDef {
			get { return false; }
		}

		bool IMemberRef.IsEventDef {
			get { return false; }
		}

		bool IMemberRef.IsGenericParam {
			get { return false; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="FieldOffset"/> is not <c>null</c>
		/// </summary>
		public bool HasLayoutInfo {
			get { return FieldOffset != null; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="Constant"/> is not <c>null</c>
		/// </summary>
		public bool HasConstant {
			get { return Constant != null; }
		}

		/// <summary>
		/// Gets the constant element type or <see cref="dnlib.DotNet.ElementType.End"/> if there's no constant
		/// </summary>
		public ElementType ElementType {
			get {
				var c = Constant;
				return c == null ? ElementType.End : c.Type;
			}
		}

		/// <summary>
		/// <c>true</c> if <see cref="MarshalType"/> is not <c>null</c>
		/// </summary>
		public bool HasMarshalType {
			get { return MarshalType != null; }
		}

		/// <summary>
		/// Gets/sets the field type
		/// </summary>
		public TypeSig FieldType {
			get { return FieldSig.GetFieldType(); }
			set {
				var sig = FieldSig;
				if (sig != null)
					sig.Type = value;
			}
		}

		/// <summary>
		/// Modify <see cref="Attributes_NoLock"/> property: <see cref="Attributes_NoLock"/> =
		/// (<see cref="Attributes_NoLock"/> &amp; <paramref name="andMask"/>) | <paramref name="orMask"/>.
		/// </summary>
		/// <param name="andMask">Value to <c>AND</c></param>
		/// <param name="orMask">Value to OR</param>
		void ModifyAttributes(FieldAttributes andMask, FieldAttributes orMask) {
#if THREAD_SAFE
			theLock.EnterWriteLock(); try {
#endif
				Attributes_NoLock = (Attributes_NoLock & andMask) | orMask;
#if THREAD_SAFE
			} finally { theLock.ExitWriteLock(); }
#endif
		}

		/// <summary>
		/// Set or clear flags in <see cref="Attributes_NoLock"/>
		/// </summary>
		/// <param name="set"><c>true</c> if flags should be set, <c>false</c> if flags should
		/// be cleared</param>
		/// <param name="flags">Flags to set or clear</param>
		void ModifyAttributes(bool set, FieldAttributes flags) {
#if THREAD_SAFE
			theLock.EnterWriteLock(); try {
#endif
				if (set)
					Attributes_NoLock |= flags;
				else
					Attributes_NoLock &= ~flags;
#if THREAD_SAFE
			} finally { theLock.ExitWriteLock(); }
#endif
		}

		/// <summary>
		/// Gets/sets the field access
		/// </summary>
		public FieldAttributes Access {
			get { return Attributes & FieldAttributes.FieldAccessMask; }
			set { ModifyAttributes(~FieldAttributes.FieldAccessMask, value & FieldAttributes.FieldAccessMask); }
		}

		/// <summary>
		/// <c>true</c> if <see cref="FieldAttributes.PrivateScope"/> is set
		/// </summary>
		public bool IsCompilerControlled {
			get { return IsPrivateScope; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="FieldAttributes.PrivateScope"/> is set
		/// </summary>
		public bool IsPrivateScope {
			get { return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.PrivateScope; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="FieldAttributes.Private"/> is set
		/// </summary>
		public bool IsPrivate {
			get { return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Private; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="FieldAttributes.FamANDAssem"/> is set
		/// </summary>
		public bool IsFamilyAndAssembly {
			get { return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamANDAssem; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="FieldAttributes.Assembly"/> is set
		/// </summary>
		public bool IsAssembly {
			get { return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Assembly; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="FieldAttributes.Family"/> is set
		/// </summary>
		public bool IsFamily {
			get { return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Family; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="FieldAttributes.FamORAssem"/> is set
		/// </summary>
		public bool IsFamilyOrAssembly {
			get { return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.FamORAssem; }
		}

		/// <summary>
		/// <c>true</c> if <see cref="FieldAttributes.Public"/> is set
		/// </summary>
		public bool IsPublic {
			get { return (Attributes & FieldAttributes.FieldAccessMask) == FieldAttributes.Public; }
		}

		/// <summary>
		/// Gets/sets the <see cref="FieldAttributes.Static"/> bit
		/// </summary>
		public bool IsStatic {
			get { return (Attributes & FieldAttributes.Static) != 0; }
			set { ModifyAttributes(value, FieldAttributes.Static); }
		}

		/// <summary>
		/// Gets/sets the <see cref="FieldAttributes.InitOnly"/> bit
		/// </summary>
		public bool IsInitOnly {
			get { return (Attributes & FieldAttributes.InitOnly) != 0; }
			set { ModifyAttributes(value, FieldAttributes.InitOnly); }
		}

		/// <summary>
		/// Gets/sets the <see cref="FieldAttributes.Literal"/> bit
		/// </summary>
		public bool IsLiteral {
			get { return (Attributes & FieldAttributes.Literal) != 0; }
			set { ModifyAttributes(value, FieldAttributes.Literal); }
		}

		/// <summary>
		/// Gets/sets the <see cref="FieldAttributes.NotSerialized"/> bit
		/// </summary>
		public bool IsNotSerialized {
			get { return (Attributes & FieldAttributes.NotSerialized) != 0; }
			set { ModifyAttributes(value, FieldAttributes.NotSerialized); }
		}

		/// <summary>
		/// Gets/sets the <see cref="FieldAttributes.SpecialName"/> bit
		/// </summary>
		public bool IsSpecialName {
			get { return (Attributes & FieldAttributes.SpecialName) != 0; }
			set { ModifyAttributes(value, FieldAttributes.SpecialName); }
		}

		/// <summary>
		/// Gets/sets the <see cref="FieldAttributes.PinvokeImpl"/> bit
		/// </summary>
		public bool IsPinvokeImpl {
			get { return (Attributes & FieldAttributes.PinvokeImpl) != 0; }
			set { ModifyAttributes(value, FieldAttributes.PinvokeImpl); }
		}

		/// <summary>
		/// Gets/sets the <see cref="FieldAttributes.RTSpecialName"/> bit
		/// </summary>
		public bool IsRuntimeSpecialName {
			get { return (Attributes & FieldAttributes.RTSpecialName) != 0; }
			set { ModifyAttributes(value, FieldAttributes.RTSpecialName); }
		}

		/// <summary>
		/// Gets/sets the <see cref="FieldAttributes.HasFieldMarshal"/> bit
		/// </summary>
		public bool HasFieldMarshal {
			get { return (Attributes & FieldAttributes.HasFieldMarshal) != 0; }
			set { ModifyAttributes(value, FieldAttributes.HasFieldMarshal); }
		}

		/// <summary>
		/// Gets/sets the <see cref="FieldAttributes.HasDefault"/> bit
		/// </summary>
		public bool HasDefault {
			get { return (Attributes & FieldAttributes.HasDefault) != 0; }
			set { ModifyAttributes(value, FieldAttributes.HasDefault); }
		}

		/// <summary>
		/// Gets/sets the <see cref="FieldAttributes.HasFieldRVA"/> bit
		/// </summary>
		public bool HasFieldRVA {
			get { return (Attributes & FieldAttributes.HasFieldRVA) != 0; }
			set { ModifyAttributes(value, FieldAttributes.HasFieldRVA); }
		}

		/// <summary>
		/// Returns the full name of this field
		/// </summary>
		public string FullName {
			get {
				var dt = DeclaringType;
				return FullNameCreator.FieldFullName(dt == null ? null : dt.FullName, Name, FieldSig);
			}
		}

		/// <summary>
		/// Gets the size of this field in bytes or <c>0</c> if unknown.
		/// </summary>
		public uint GetFieldSize() {
			uint size;
			if (!GetFieldSize(out size))
				return 0;
			return size;
		}

		/// <summary>
		/// Gets the size of this field in bytes or <c>0</c> if unknown.
		/// </summary>
		/// <param name="size">Updated with size</param>
		/// <returns><c>true</c> if <paramref name="size"/> is valid, <c>false</c> otherwise</returns>
		public bool GetFieldSize(out uint size) {
			return GetFieldSize(DeclaringType, FieldSig, out size);
		}

		/// <summary>
		/// Gets the size of this field in bytes or <c>0</c> if unknown.
		/// </summary>
		/// <param name="declaringType">The declaring type of <c>this</c></param>
		/// <param name="fieldSig">The field signature of <c>this</c></param>
		/// <param name="size">Updated with size</param>
		/// <returns><c>true</c> if <paramref name="size"/> is valid, <c>false</c> otherwise</returns>
		protected bool GetFieldSize(TypeDef declaringType, FieldSig fieldSig, out uint size) {
			size = 0;
			if (fieldSig == null)
				return false;
			return GetClassSize(declaringType, fieldSig.Type, out size);
		}

		bool GetClassSize(TypeDef declaringType, TypeSig ts, out uint size) {
			size = 0;
			ts = ts.RemovePinnedAndModifiers();
			if (ts == null)
				return false;

			int size2 = ts.ElementType.GetPrimitiveSize(GetPointerSize(declaringType));
			if (size2 >= 0) {
				size = (uint)size2;
				return true;
			}

			var tdrs = ts as TypeDefOrRefSig;
			if (tdrs == null)
				return false;

			var td = tdrs.TypeDef;
			if (td != null)
				return TypeDef.GetClassSize(td, out size);

			var tr = tdrs.TypeRef;
			if (tr != null)
				return TypeDef.GetClassSize(tr.Resolve(), out size);

			return false;
		}

		int GetPointerSize(TypeDef declaringType) {
			if (declaringType == null)
				return 4;
			var module = declaringType.Module;
			if (module == null)
				return 4;
			return module.GetPointerSize();
		}

		/// <inheritdoc/>
		public override string ToString() {
			return FullName;
		}
	}

	/// <summary>
	/// A Field row created by the user and not present in the original .NET file
	/// </summary>
	public class FieldDefUser : FieldDef {
		readonly CustomAttributeCollection customAttributeCollection = new CustomAttributeCollection();
		FieldAttributes flags;
		UTF8String name;
		CallingConventionSig signature;
		uint? fieldOffset;
		MarshalType marshalType;
		RVA rva;
		byte[] initialValue;
		ImplMap implMap;
		Constant constant;
		TypeDef declaringType;

		/// <inheritdoc/>
		public override CustomAttributeCollection CustomAttributes {
			get { return customAttributeCollection; }
		}

		/// <inheritdoc/>
		protected override FieldAttributes Attributes_NoLock {
			get { return flags; }
			set { flags = value; }
		}

		/// <inheritdoc/>
		public override UTF8String Name {
			get { return name; }
			set { name = value; }
		}

		/// <inheritdoc/>
		public override CallingConventionSig Signature {
			get { return signature; }
			set { signature = value; }
		}

		/// <inheritdoc/>
		public override uint? FieldOffset {
			get { return fieldOffset; }
			set { fieldOffset = value; }
		}

		/// <inheritdoc/>
		public override MarshalType MarshalType {
			get { return marshalType; }
			set { marshalType = value; }
		}

		/// <inheritdoc/>
		public override RVA RVA {
			get { return rva; }
			set { rva = value; }
		}

		/// <inheritdoc/>
		public override byte[] InitialValue {
			get { return initialValue; }
			set { initialValue = value; }
		}

		/// <inheritdoc/>
		public override ImplMap ImplMap {
			get { return implMap; }
			set { implMap = value; }
		}

		/// <inheritdoc/>
		public override Constant Constant {
			get { return constant; }
			set { constant = value; }
		}

		/// <inheritdoc/>
		internal override TypeDef DeclaringType2_NoLock {
			get { return declaringType; }
			set { declaringType = value; }
		}

		/// <summary>
		/// Default constructor
		/// </summary>
		public FieldDefUser() {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name</param>
		public FieldDefUser(UTF8String name)
			: this(name, null) {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="signature">Signature</param>
		public FieldDefUser(UTF8String name, FieldSig signature)
			: this(name, signature, 0) {
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="signature">Signature</param>
		/// <param name="flags">Flags</param>
		public FieldDefUser(UTF8String name, FieldSig signature, FieldAttributes flags) {
			this.name = name;
			this.signature = signature;
			this.flags = flags;
		}
	}

	/// <summary>
	/// Created from a row in the Field table
	/// </summary>
	sealed class FieldDefMD : FieldDef, IMDTokenProviderMD {
		/// <summary>The module where this instance is located</summary>
		readonly ModuleDefMD readerModule;
		/// <summary>The raw table row. It's <c>null</c> until <see cref="InitializeRawRow_NoLock"/> is called</summary>
		RawFieldRow rawRow;

		readonly uint origRid;
		CustomAttributeCollection customAttributeCollection;
		UserValue<FieldAttributes> flags;
		UserValue<UTF8String> name;
		UserValue<CallingConventionSig> signature;
		UserValue<uint?> fieldOffset;
		UserValue<MarshalType> marshalType;
		UserValue<RVA> rva;
		UserValue<byte[]> initialValue;
		UserValue<ImplMap> implMap;
		UserValue<Constant> constant;
		UserValue<TypeDef> declaringType;

		/// <inheritdoc/>
		public uint OrigRid {
			get { return origRid; }
		}

		/// <inheritdoc/>
		public override CustomAttributeCollection CustomAttributes {
			get {
				if (customAttributeCollection == null) {
					var list = readerModule.MetaData.GetCustomAttributeRidList(Table.Field, origRid);
					var tmp = new CustomAttributeCollection((int)list.Length, list, (list2, index) => readerModule.ReadCustomAttribute(((RidList)list2)[index]));
					Interlocked.CompareExchange(ref customAttributeCollection, tmp, null);
				}
				return customAttributeCollection;
			}
		}

		/// <inheritdoc/>
		protected override FieldAttributes Attributes_NoLock {
			get { return flags.Value; }
			set { flags.Value = value; }
		}

		/// <inheritdoc/>
		public override UTF8String Name {
			get { return name.Value; }
			set { name.Value = value; }
		}

		/// <inheritdoc/>
		public override CallingConventionSig Signature {
#if THREAD_SAFE
			get {
				theLock.EnterWriteLock(); try {
					return Signature_NoLock;
				} finally { theLock.ExitWriteLock(); }
			}
			set {
				theLock.EnterWriteLock(); try {
					Signature_NoLock = value;
				} finally { theLock.ExitWriteLock(); }
			}
#else
			get { return Signature_NoLock; }
			set { Signature_NoLock = value; }
#endif
		}

		CallingConventionSig Signature_NoLock {
			get { return signature.Value; }
			set { signature.Value = value; }
		}

		/// <inheritdoc/>
		public override uint? FieldOffset {
			get { return fieldOffset.Value; }
			set { fieldOffset.Value = value; }
		}

		/// <inheritdoc/>
		public override MarshalType MarshalType {
			get { return marshalType.Value; }
			set { marshalType.Value = value; }
		}

		/// <inheritdoc/>
		public override RVA RVA {
			get { return rva.Value; }
			set { rva.Value = value; }
		}

		/// <inheritdoc/>
		public override byte[] InitialValue {
			get { return initialValue.Value; }
			set { initialValue.Value = value; }
		}

		/// <inheritdoc/>
		public override ImplMap ImplMap {
			get { return implMap.Value; }
			set { implMap.Value = value; }
		}

		/// <inheritdoc/>
		public override Constant Constant {
			get { return constant.Value; }
			set { constant.Value = value; }
		}

		/// <inheritdoc/>
		internal override TypeDef DeclaringType2_NoLock {
			get { return declaringType.Value; }
			set { declaringType.Value = value; }
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="readerModule">The module which contains this <c>Field</c> row</param>
		/// <param name="rid">Row ID</param>
		/// <exception cref="ArgumentNullException">If <paramref name="readerModule"/> is <c>null</c></exception>
		/// <exception cref="ArgumentException">If <paramref name="rid"/> is invalid</exception>
		public FieldDefMD(ModuleDefMD readerModule, uint rid) {
#if DEBUG
			if (readerModule == null)
				throw new ArgumentNullException("readerModule");
			if (readerModule.TablesStream.FieldTable.IsInvalidRID(rid))
				throw new BadImageFormatException(string.Format("Field rid {0} does not exist", rid));
#endif
			this.origRid = rid;
			this.rid = rid;
			this.readerModule = readerModule;
			Initialize();
		}

		void Initialize() {
			flags.ReadOriginalValue = () => {
				InitializeRawRow_NoLock();
				return (FieldAttributes)rawRow.Flags;
			};
			name.ReadOriginalValue = () => {
				InitializeRawRow_NoLock();
				return readerModule.StringsStream.ReadNoNull(rawRow.Name);
			};
			signature.ReadOriginalValue = () => {
				InitializeRawRow_NoLock();
				return readerModule.ReadSignature(rawRow.Signature);
			};
			fieldOffset.ReadOriginalValue = () => {
				var row = readerModule.TablesStream.ReadFieldLayoutRow(readerModule.MetaData.GetFieldLayoutRid(origRid));
				return row == null ? null : new uint?(row.OffSet);
			};
			marshalType.ReadOriginalValue = () => {
				return readerModule.ReadMarshalType(Table.Field, origRid);
			};
			rva.ReadOriginalValue = () => {
				RVA rva2;
				GetFieldRVA_NoLock(out rva2);
				return rva2;
			};
			initialValue.ReadOriginalValue = () => {
				RVA rva2;
				if (!GetFieldRVA_NoLock(out rva2))
					return null;
				return ReadInitialValue_NoLock(rva2);
			};
			implMap.ReadOriginalValue = () => {
				return readerModule.ResolveImplMap(readerModule.MetaData.GetImplMapRid(Table.Field, origRid));
			};
			constant.ReadOriginalValue = () => {
				return readerModule.ResolveConstant(readerModule.MetaData.GetConstantRid(Table.Field, origRid));
			};
			declaringType.ReadOriginalValue = () => {
				return readerModule.GetOwnerType(this);
			};
#if THREAD_SAFE
			// flags.Lock = theLock;			No lock for this one
			name.Lock = theLock;
			// signature.Lock = theLock;		No lock for this one
			fieldOffset.Lock = theLock;
			marshalType.Lock = theLock;
			rva.Lock = theLock;
			initialValue.Lock = theLock;
			implMap.Lock = theLock;
			constant.Lock = theLock;
			// declaringType.Lock = theLock;	No lock for this one
#endif
		}

		void InitializeRawRow_NoLock() {
			if (rawRow != null)
				return;
			rawRow = readerModule.TablesStream.ReadFieldRow(origRid);
		}

		internal FieldDefMD InitializeAll() {
			MemberMDInitializer.Initialize(CustomAttributes);
			MemberMDInitializer.Initialize(Attributes);
			MemberMDInitializer.Initialize(Name);
			MemberMDInitializer.Initialize(Signature);
			MemberMDInitializer.Initialize(FieldOffset);
			MemberMDInitializer.Initialize(MarshalType);
			MemberMDInitializer.Initialize(RVA);
			MemberMDInitializer.Initialize(InitialValue);
			MemberMDInitializer.Initialize(ImplMap);
			MemberMDInitializer.Initialize(Constant);
			MemberMDInitializer.Initialize(DeclaringType);
			return this;
		}

		bool GetFieldRVA_NoLock(out RVA rva) {
			InitializeRawRow_NoLock();
			if (((FieldAttributes)rawRow.Flags & FieldAttributes.HasFieldRVA) == 0) {
				rva = 0;
				return false;
			}
			var row = readerModule.TablesStream.ReadFieldRVARow(readerModule.MetaData.GetFieldRVARid(origRid));
			if (row == null) {
				rva = 0;
				return false;
			}
			rva = (RVA)row.RVA;
			return true;
		}

		byte[] ReadInitialValue_NoLock(RVA rva) {
			uint size;
			if (!GetFieldSize(DeclaringType2_NoLock, Signature_NoLock as FieldSig, out size))
				return null;
			if (size >= int.MaxValue)
				return null;
			return readerModule.ReadDataAt(rva, (int)size);
		}
	}
}
