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
	}
}
