using System;
using System.Collections.Generic;
using System.Text;

namespace chatme.Domain.Common
{

	public abstract class BaseEntity
	{
		public Guid Id { get; protected set; } = Guid.NewGuid();

		public override bool Equals(object? obj)
		{
			if (obj is not BaseEntity other) return false;
			if (ReferenceEquals(this, other)) return true;
			if (GetType() != other.GetType()) return false;
			return Id == other.Id;
		}

		public override int GetHashCode() => (GetType().ToString() + Id).GetHashCode();

		public static bool operator ==(BaseEntity? left, BaseEntity? right) =>
			left is null ? right is null : left.Equals(right);

		public static bool operator !=(BaseEntity? left, BaseEntity? right) => !(left == right);
	}
}
