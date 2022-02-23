﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATNC;

public class RoadsWrapper {
	public readonly double x, y, w, h, angle;
	public readonly Type type;
	public readonly string name;

	public static readonly Dictionary<char, Type> types = new() {
		{ 'n', typeof(Road) },
		{ 'b', typeof(Bridge) },
		{ 't', typeof(Tunnel) }
	};

	public RoadsWrapper(Type type, double x, double y, double w, double h, double angle, string name) {
		this.type = type;
		this.x = x;
		this.y = y;
		this.w = w;
		this.h = h;
		this.angle = angle;
		this.name = name;
	}

	public RoadsWrapper(string str) {
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
	}
}