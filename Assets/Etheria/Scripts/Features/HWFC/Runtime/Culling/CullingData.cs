namespace Etheria.Features.HWFC {
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

    [RequireComponent(typeof(MapBehaviour))]
    public class CullingData : MonoBehaviour {
	[HideInInspector]
	public MapBehaviour MapBehaviour;

	public Dictionary<Vector3Int, Room> RoomsByPosition;

	private Dictionary<Vector3Int, Portal[]> portalsByPosition;

	private HashSet<Vector3Int> outdatedSlots;

	public Dictionary<Vector3Int, Chunk> Chunks;
	public List<Chunk> ChunksInRange;

	public int ChunkSize = 3;

	public bool DrawGizmo = false;

	public void Initialize() {
		MapBehaviour = GetComponent<MapBehaviour>();
		RoomsByPosition = new Dictionary<Vector3Int, Room>();
		portalsByPosition = new Dictionary<Vector3Int, Portal[]>();
		outdatedSlots = new HashSet<Vector3Int>();
		Chunks = new Dictionary<Vector3Int, Chunk>();
		ChunksInRange = new List<Chunk>();
	}

	public Vector3Int GetChunkAddress(Vector3Int position) {
		return Vector3Int.FloorToInt(position.ToVector3() / ChunkSize);
	}

	public Vector3 GetChunkCenter(Vector3Int chunkAddress) {
		return MapBehaviour.GetWorldspacePosition(chunkAddress * ChunkSize) + (ChunkSize - 1) * 0.5f * AbstractMap.BLOCK_SIZE * Vector3.one;
	}

	private Chunk GetChunk(Vector3Int chunkAddress) {
		if (Chunks.ContainsKey(chunkAddress)) {
			return Chunks[chunkAddress];
		}
		var chunk = new Chunk(new Bounds(GetChunkCenter(chunkAddress), Vector3.one * AbstractMap.BLOCK_SIZE * ChunkSize));
		Chunks[chunkAddress] = chunk;
		ChunksInRange.Add(chunk);
		return chunk;
	}

	public Chunk GetChunkFromPosition(Vector3Int position) {
		return GetChunk(GetChunkAddress(position));
	}

	public Room GetRoom(Vector3Int position) {
		if (RoomsByPosition.ContainsKey(position)) {
			return RoomsByPosition[position];
		} else {
			return null;
		}
	}

	public void AddSlot(Slot slot) {
		if (!slot.Collapsed) {
			return;
		}
		var chunk = GetChunkFromPosition(slot.Position);
		if (!slot.Module.IsInterior) {
			for (int i = 0; i < 6; i++) {
				var face = slot.Module.GetFace(i);
				if (face.IsOcclusionPortal) {
					var portal = GetPortal(slot.Position, i);
					if (portal.Room != null && !portal.Room.Portals.Contains(portal)) {
						portal.Room.Portals.Add(portal);
					}
				}
			}

			chunk.AddBlock(slot);
			return;
		}

		Room room = null;
		for (int i = 0; i < 6; i++) {
			var face = slot.Module.GetFace(i);
			if (face.Connector == 1 || face.IsOcclusionPortal) {
				continue;
			}
			var neighbor = slot.GetNeighbor(i);
			if (neighbor == null) {
				continue;
			}
			if (neighbor.Collapsed && RoomsByPosition.ContainsKey(neighbor.Position) && !neighbor.Module.GetFace((i + 3) % 6).IsOcclusionPortal) {
				if (room == null) {
					room = RoomsByPosition[neighbor.Position];
				} if (room != RoomsByPosition[neighbor.Position]) {
					room = MergeRooms(RoomsByPosition[neighbor.Position], room);
				}
			}
		}
		if (room == null) {
			room = new Room();
			chunk.Rooms.Add(room);
		}
		room.Slots.Add(slot);
		foreach (var renderer in slot.GameObject.GetComponentsInChildren<Renderer>()) {
			room.Renderers.Add(renderer);
		}
		RoomsByPosition[slot.Position] = room;

		for (int i = 0; i < 6; i++) {
			var face = slot.Module.GetFace(i);
			if (face.Connector == 1) {
				continue;
			}
			var neighbor = slot.GetNeighbor(i);
			if (face.IsOcclusionPortal || (neighbor != null && neighbor.Collapsed && (!neighbor.Module.IsInterior || neighbor.Module.GetFace((i + 3) % 6).IsOcclusionPortal))) {
				var portal = GetPortal(slot.Position, i);
				room.Portals.Add(portal);
				var otherRoom = portal.Follow(room);
				if (otherRoom != null && !otherRoom.Portals.Contains(portal)) {
					otherRoom.Portals.Add(portal);
				}
			}
		}
		UpdatePortals(slot.Position, room);
	}

	private void UpdatePortals(Vector3Int position, Room room) {
		Portal[] portals = null;
		if (portalsByPosition.TryGetValue(position, out portals)) {
			for (int i = 0; i < 3; i++) {
				if (portals[i] != null) {
					portals[i].Room1 = room;
				}
			}
		}
		for (int i = 0; i < 3; i++) {
			var neighborPosition = position + Orientations.Direction[3 + i];
			if (portalsByPosition.TryGetValue(neighborPosition, out portals) && portals[i] != null) {
				portals[i].Room2 = room;
			}
		}
	}

	public void ClearOutdatedSlots() {
		if (!outdatedSlots.Any()) {
			return;
		}
		var items = outdatedSlots.ToArray();
		outdatedSlots.Clear();
		foreach (var position in items) {
			var slot = MapBehaviour.Map.GetSlot(position);
			if (slot == null || !slot.Collapsed) {
				continue;
			}
			AddSlot(slot);
		}
	}

	private void RemovePortal(Portal portal) {
		if (portalsByPosition.ContainsKey(portal.Position1)) {
			portalsByPosition[portal.Position1][portal.Direction] = null;
			if (portalsByPosition[portal.Position1].All(p => p == null)) {
				portalsByPosition.Remove(portal.Position1);
			}
		}
		GetChunkFromPosition(portal.Position1).Portals.Remove(portal);
		GetChunkFromPosition(portal.Position2).Portals.Remove(portal);
		if (portal.Room1 != null) {
			portal.Room1.Portals.Remove(portal);
		}
		if (portal.Room2 != null) {
			portal.Room2.Portals.Remove(portal);
		}
	}

	public void RemoveSlot(Slot slot) {
		var chunk = GetChunkFromPosition(slot.Position);
		chunk.RemoveBlock(slot);

		if (RoomsByPosition.ContainsKey(slot.Position)) {
			var room = RoomsByPosition[slot.Position];
			foreach (var portal in room.Portals.ToArray()) {
				RemovePortal(portal);
			}
			foreach (var roomSlot in room.Slots) {
				outdatedSlots.Add(roomSlot.Position);
				RoomsByPosition.Remove(roomSlot.Position);
			}
			RemoveRoom(room);
		}
		outdatedSlots.Remove(slot.Position);
	}

	private void RemoveRoom(Room room) {
		foreach (var slot in room.Slots) {
			var chunk = GetChunkFromPosition(slot.Position);
			if (chunk.Rooms.Contains(room)) {
				chunk.Rooms.Remove(room);
			}
		}
	}

	private Room MergeRooms(Room room1, Room room2) {
		foreach (var slot in room1.Slots) {
			RoomsByPosition[slot.Position] = room2;
			room2.Slots.Add(slot);
		}
		room2.Renderers.AddRange(room1.Renderers);
		room2.VisibilityOutdated = true;
		foreach (var portal in room1.Portals) {
			portal.ReplaceRoom(room1, room2);
			room2.Portals.Add(portal);
		}
		RemoveRoom(room1);
		return room2;
	}

	private void AddPortalToChunks(Portal portal) {
		var chunk1 = GetChunkFromPosition(portal.Position1);
		chunk1.Portals.Add(portal);
		var chunk2 = GetChunkFromPosition(portal.Position2);
		if (chunk2 != chunk1) {
			chunk2.Portals.Add(portal);
		}
	}

	private Portal GetPortal(Vector3Int position, int direction) {
		if (direction >= 3) {
			position = position + Orientations.Direction[direction];
			direction -= 3;
		}
		if (portalsByPosition.ContainsKey(position)) {
			var array = portalsByPosition[position];
			if (array[direction] == null) {
				var portal = new Portal(position, direction, this);
				array[direction] = portal;
				AddPortalToChunks(portal);
				return portal;
			} else {
				return array[direction];
			}
		} else {
			var portal = new Portal(position, direction, this);
			var array = new Portal[3];
			array[direction] = portal;
			portalsByPosition[position] = array;
			AddPortalToChunks(portal);
			return portal;
		}
	}

#if UNITY_EDITOR
	[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
	static void DrawGizmos(CullingData cullingData, GizmoType gizmoType) {
		if (!cullingData.DrawGizmo || cullingData.ChunksInRange == null) {
			return;
		}
		foreach (var chunk in cullingData.ChunksInRange) {
			foreach (var room in chunk.Rooms) {
				room.DrawGizmo(cullingData.MapBehaviour);
			}
		}
	}
#endif
}
}
