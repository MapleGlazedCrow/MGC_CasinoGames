using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CardGamesLibrary
{
	/// <summary>
	/// Represents a unique identifier for a client, player, or entity.
	/// Currently wraps a Guid but can later be swapped to another strategy
	/// without changing external code.
	/// </summary>
	public sealed class Identifier : IEquatable<Identifier>
	{
		private readonly Guid _value;

		/// <summary>
		/// Creates a new unique Identifier.
		/// </summary>
		public Identifier()
		{
			_value = Guid.Empty;
		}

		/// <summary>
		/// Creates an Identifier from an existing Guid.
		/// </summary>
		[JsonConstructor]
		public Identifier(Guid value)
		{
			_value = value;
		}

		/// <summary>
		/// Creates an Identifier from an existing Guid.
		/// </summary>
		public Identifier(string value)
		{
			_value = Guid.Parse(value);
		}

		/// <summary>
		/// Returns the underlying Guid. Useful if you need interoperability.
		/// </summary>
		public Guid Value => _value;

		/// <summary>
		/// Create an Identifier from a string (e.g. received over network).
		/// </summary>
		public static Identifier Parse(string input) =>
			new(Guid.Parse(input));

		/// <summary>
		/// Try parsing without throwing.
		/// </summary>
		public static bool TryParse(string input, out Identifier? id)
		{
			if(Guid.TryParse(input, out var guid))
			{
				id = new Identifier(guid);
				return true;
			}

			id = null;
			return false;
		}

		/// <inheritdoc/>
		public override string ToString() => _value.ToString();

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as Identifier);

		/// <inheritdoc/>
		public bool Equals(Identifier? other) =>
			other is not null && _value.Equals(other._value);

		/// <inheritdoc/>
		public override int GetHashCode() => _value.GetHashCode();

		public static bool operator ==(Identifier? left, Identifier? right) =>
			Equals(left, right);

		public static bool operator !=(Identifier? left, Identifier? right) =>
			!Equals(left, right);

		/// <summary>
		/// Create a new Identifier.
		/// </summary>
		public static Identifier New() => new Identifier(Guid.NewGuid());
	}
}
