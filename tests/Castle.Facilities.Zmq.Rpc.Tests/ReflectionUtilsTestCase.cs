namespace Castle.Facilities.Zmq.Rpc.Tests
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using Castle.Facilities.Zmq.Rpc.Internal;
	using FluentAssertions;
	using NUnit.Framework;

	[TestFixture]
	public class ReflectionUtilsTestCase
	{
		[Test]
		public void IsCollectionType_for_non_array_types()
		{
			ReflectionUtils.IsCollectionType(typeof (string)).Should().BeFalse();
			ReflectionUtils.IsCollectionType(typeof(DateTime)).Should().BeFalse();
			ReflectionUtils.IsCollectionType(typeof(Int16)).Should().BeFalse();
		}

		[Test]
		public void IsCollectionType_for_array_types()
		{
			ReflectionUtils.IsCollectionType(typeof(IEnumerable<string>)).Should().BeTrue();
			ReflectionUtils.IsCollectionType(typeof(IList<string>)).Should().BeTrue();
			ReflectionUtils.IsCollectionType(typeof(string[])).Should().BeTrue();
			ReflectionUtils.IsCollectionType(typeof(IList)).Should().BeTrue();
		}

		[Test]
		public void NormalizeToArray_for_arrays()
		{
			var res = ReflectionUtils.NormalizeToArray(new [] {"1", "2"}, typeof (string[]));
			res.Should().BeOfType<string[]>();
		}

		[Test]
		public void NormalizeToArray_for_lists()
		{
			var res = ReflectionUtils.NormalizeToArray(new ArrayList(new[] { "1", "2" }), typeof(ArrayList));
			res.Should().BeOfType<object[]>();
		}

		[Test]
		public void MakeStronglyTypedArray_for_primitives()
		{
			var res = ReflectionUtils.MakeStronglyTypedArray(typeof (int), new ArrayList(new[] {1, 2, 3}));
			res.Should().BeOfType<int[]>();
		}

		[Test]
		public void MakeStronglyTypedArray_for_non_primitives()
		{
			var res = ReflectionUtils.MakeStronglyTypedArray(typeof(string), new ArrayList(new[] { "1", "2", "3" }));
			res.Should().BeOfType<string[]>();
		}

		[Test]
		public void MakeStronglyTypedEnumerable()
		{
			var res = ReflectionUtils.MakeStronglyTypedEnumerable(typeof(int), new ArrayList(new[] { 1, 2, 3 }));
			res.Should().BeAssignableTo<IList<int>>();
		}
	}
}
