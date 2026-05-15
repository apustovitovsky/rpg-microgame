namespace Etheria.Features.HWFC {
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// A finite sized map that uses horizontal world wrapping.
/// That means you can horizontally tile copies of this map and the edges will match
/// </summary>
public class TilingMap : AbstractMap {
	public readonly Vector3Int Size;

	private readonly Slot[,,] slots;

	public TilingMap(Vector3Int size) : base() {
		Size = size;
		slots = new Slot[size.x, size.y, size.z];

		for (int x = 0; x < size.x; x++) {
			for (int y = 0; y < size.y; y++) {
				for (int z = 0; z < size.z; z++) {
					slots[x,y,z] = new Slot(new Vector3Int(x,y,z), this);
				}
			}
		}
	}

	public override Slot GetSlot(Vector3Int position) {
		if (position.y < 0 || position.y >= Size.y) {
			return null;
		}
		return slots[
			position.x % Size.x + (position.x % Size.x < 0 ? Size.x : 0),
			position.y,
			position.z % Size.z + (position.z % Size.z < 0 ? Size.z : 0)];
	}

	public override IEnumerable<Slot> GetAllSlots() {
		for (int x = 0; x < Size.x; x++) {
			for (int y = 0; y < Size.y; y++) {
				for (int z = 0; z < Size.z; z++) {
					yield return slots[x, y, z];
				}
			}
		}
	}

	public override void ApplyBoundaryConstraints(IEnumerable<BoundaryConstraint> constraints) {
		foreach (var constraint in constraints) {
			int y = constraint.RelativeY;
			if (y < 0) {
				y += Size.y;
			}
			switch (constraint.Direction) {
				case BoundaryConstraint.ConstraintDirection.Up:
					for (int x = 0; x < Size.x; x++) {
						for (int z = 0; z < Size.z; z++) {
							if (constraint.Mode == BoundaryConstraint.ConstraintMode.EnforceConnector) {
								GetSlot(new Vector3Int(x, Size.y - 1, z)).EnforceConnector(4, constraint.Connector);
							} else {
								GetSlot(new Vector3Int(x, Size.y - 1, z)).ExcludeConnector(4, constraint.Connector);
							}
						}
					}
					break;
				case BoundaryConstraint.ConstraintDirection.Down:
					for (int x = 0; x < Size.x; x++) {
						for (int z = 0; z < Size.z; z++) {
							if (constraint.Mode == BoundaryConstraint.ConstraintMode.EnforceConnector) {
								GetSlot(new Vector3Int(x, 0, z)).EnforceConnector(1, constraint.Connector);
							} else {
								GetSlot(new Vector3Int(x, 0, z)).ExcludeConnector(1, constraint.Connector);
							}
						}
					}
					break;
				case BoundaryConstraint.ConstraintDirection.Horizontal:
					// Horizontal constraints are ignored
					break;
			}
		}
	}
}
}

