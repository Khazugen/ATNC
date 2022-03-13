using System;
using System.Collections.Generic;
using System.Linq;

namespace ATNC;

public class RoadWrapper {
	public readonly double x, w, h;
	public readonly Type type;
	public readonly string name;
	private readonly uint _id;

	public static readonly Dictionary<char, Type> types = new() {
		{ 'n', typeof(Road) },
		{ 'b', typeof(Bridge) },
		{ 't', typeof(Tunnel) }
	};

	private static uint _sid = 0;

	public RoadWrapper(Type type, double x, double w, double h, string name) {
		this.type = type;
		this.x = x;
		this.w = w;
		this.h = h;
		this.name = name;
		_id = _sid++;
	}

	public RoadWrapper(string str) {
		type = types[str[0]];

		double[] spld = str
			.Remove(0, 1)
			.Split(';')
			.SkipLast(1)
			.Select(x => x
				.Split('+')
				.Select(s => double.Parse(s))
				.Sum())
			.ToArray();

		x = spld[0];
		w = spld[1];
		h = spld[2];
		name = str.Split(';')[^1];

		if (name == "_")
			name = null;

		_id = _sid++;
	}

	public override string ToString() => $"x - {x}; width - {w}; height - {h}; name - {name}";
	public override bool Equals(object obj) =>
		obj is RoadWrapper wrapper
		&& x == wrapper.x
		&& w == wrapper.w
		&& h == wrapper.h
		&& EqualityComparer<Type>.Default.Equals(type, wrapper.type)
		&& name == wrapper.name;

	public override int GetHashCode() => HashCode.Combine(_id);
}