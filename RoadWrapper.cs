using System;
using System.Collections.Generic;
using System.Linq;

namespace ATNC;

public class RoadWrapper {
	public readonly double x, y, w, h, angle;
	public readonly Type type;
	public readonly string name;
	private readonly uint _id;

	public static readonly Dictionary<char, Type> types = new() {
		{ 'n', typeof(Road) },
		{ 'b', typeof(Bridge) },
		{ 't', typeof(Tunnel) }
	};

	private static uint _sid = 0;

	public RoadWrapper(Type type, double x, double y, double w, double h, double angle, string name) {
		this.type = type;
		this.x = x;
		this.y = y;
		this.w = w;
		this.h = h;
		this.angle = angle;
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
		y = spld[1];
		w = spld[2];
		h = spld[3];
		angle = spld[4];
		name = str.Split(';')[^1];

		if (name == "_")
			name = null;
		_id = _sid++;
	}

	public override string ToString() => $"x - {x}; y - {y}; width - {w}; height - {h}; angle - {angle}; name - {name}";
	public override bool Equals(object obj) =>
		obj is RoadWrapper wrapper
		&& x == wrapper.x
		&& y == wrapper.y
		&& w == wrapper.w
		&& h == wrapper.h
		&& angle == wrapper.angle
		&& EqualityComparer<Type>.Default.Equals(type, wrapper.type)
		&& name == wrapper.name;

	public override int GetHashCode() => (int)_id;
}