namespace RpcFacilityStressTest
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using Castle.Zmq.Rpc;
	using ProtoBuf;

	[ProtoContract]
	public struct MyCustomStruct
	{
		[ProtoMember(1)]
		public string Name;
		[ProtoMember(2)]
		public int Age;
	}

	[ProtoContract]
	public class Impl1
	{
	}

	public interface IContract1
	{
		string Name { get; set; }
		int Age { get; set; }
	}
	[ProtoContract]
	public class Contract1Impl : IContract1
	{
		[ProtoMember(1)]
		public string Name { get; set; }
		[ProtoMember(2)]
		public int Age { get; set; }
	}

	[RemoteService]
	public interface IRemoteServ1
	{
		void NoParamsOrReturn();
		string JustReturn();
		void JustParams(string p1);
		void ParamWithArray(string[] p1);
		void ParamWithArray2(int[] p1);
		void ParamWithArray3(Derived2[] p1);
		void ParamWithArray4(Base[] p1);
		string ParamsAndReturn(Guid id, string p1, int p2, DateTime dt, decimal p4, FileAccess acc, short s1, byte b1, float f1, double d1);
		void ParamsWithStruct(MyCustomStruct p1);
		void ParamsWithCustomType1(Impl1 p1);
		void ParamsWithCustomType2(IContract1 p1);
		void WithInheritanceParam(Base b);
		Base WithInheritanceRet();
		IEnumerable<Derived1> UsingEnumerators();
		Derived1[] UsingArray();
		void DoSomethingWrong();

		Base ReturningNull1();
		string ReturningNull2();
	}

	[ProtoContract]
	[ProtoInclude(1, typeof(Derived1))]
	[ProtoInclude(2, typeof(Derived2))]
	public class Base
	{
		[ProtoMember(10)]
		public int Something { get; set; }
	}
	[ProtoContract]
	public class Derived1 : Base
	{
		[ProtoMember(20)]
		public int DerivedProp1 { get; set; }
	}
	[ProtoContract]
	public class Derived2 : Base
	{
		[ProtoMember(30)]
		public string DerivedProp2 { get; set; }
	}

	[ProtoContract]
	public class Contract2Impl : IContract1
	{
		[ProtoMember(1)]
		public string Name { get; set; }
		[ProtoMember(2)]
		public int Age { get; set; }
	}

}
