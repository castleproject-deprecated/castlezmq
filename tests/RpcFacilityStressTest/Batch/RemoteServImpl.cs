namespace RpcFacilityStressTest
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using NUnit.Framework;

	public class RemoteServImpl : IRemoteServ1
	{
		public void NoParamsOrReturn()
		{
		}

		public string JustReturn()
		{
			return "abc";
		}

		public void JustParams(string p1)
		{
			Assert.IsNotNull(p1);
		}

		public string ParamsAndReturn(Guid id, string p1, int p2, DateTime dt, decimal p4, FileAccess acc, short s1, byte b1, float f1, double d1)
		{
			return string.Empty;
		}

		public void ParamsWithStruct(MyCustomStruct p1)
		{
			Assert.IsNotNull(p1.Name);
		}

		public void ParamsWithCustomType1(Impl1 p1)
		{
			Assert.IsNotNull(p1);
		}

		public void ParamsWithCustomType2(IContract1 p1)
		{
			Assert.IsNotNull(p1);
		}

		public void WithInheritanceParam(Base b)
		{
			Assert.IsNotNull(b);
		}

		public Base WithInheritanceRet()
		{
			return new Derived2()
			{
				Something = 10,
				DerivedProp2 = "test"
			};
		}

		public IEnumerable<Derived1> UsingEnumerators()
		{
			return new[] { new Derived1() { Something = 10 }, new Derived1() { Something = 11 }, };
		}

		public Derived1[] UsingArray()
		{
			return new[] { new Derived1() { Something = 10 }, new Derived1() { Something = 11 }, };
		}

		public void DoSomethingWrong()
		{
			throw new Exception("simple message");
		}

		public void ParamWithArray(string[] p1)
		{
			Assert.NotNull(p1);
			Assert.AreEqual(3, p1.Length);
		}

		public void ParamWithArray2(int[] p1)
		{
			Assert.NotNull(p1);
			Assert.AreEqual(3, p1.Length);
		}

		public void ParamWithArray3(Derived2[] p1)
		{
			Assert.NotNull(p1);
			Assert.AreEqual(3, p1.Length);
		}

		public void ParamWithArray4(Base[] p1)
		{
			Assert.NotNull(p1);
			Assert.AreEqual(3, p1.Length);
		}
	}
}
