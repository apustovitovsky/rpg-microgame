using System.Collections.Generic;
using System.Linq;
using Etheria.Features.HWFC;

public static class ModuleValidation {
	public static List<string> ValidateCatalog(ModuleCatalogSO catalog) {
		var errors = new List<string>();
		if (catalog == null) {
			errors.Add("Catalog is null.");
			return errors;
		}

		var items = catalog.GetValidItems().ToArray();
		if (items.Length == 0) {
			errors.Add("Catalog does not contain any module definitions.");
			return errors;
		}

		var duplicateIds = items
			.Where(item => !string.IsNullOrWhiteSpace(item.StableId))
			.GroupBy(item => item.StableId)
			.Where(group => group.Count() > 1)
			.Select(group => group.Key);

		foreach (var duplicateId in duplicateIds) {
			errors.Add("Duplicate StableId: " + duplicateId);
		}

		foreach (var definition in items) {
			errors.AddRange(ValidateDefinition(definition).Select(message => definition.name + ": " + message));
		}

		return errors;
	}

	public static List<string> ValidateDefinition(ModuleDefinitionSO definition) {
		var errors = new List<string>();
		if (definition == null) {
			errors.Add("Definition is null.");
			return errors;
		}

		if (string.IsNullOrWhiteSpace(definition.StableId)) {
			errors.Add("StableId is empty.");
		}

		if (definition.Prefab == null) {
			errors.Add("Prefab is missing.");
		}

		foreach (var face in definition.GetFaces()) {
			if (face == null) {
				errors.Add("One of the faces is null.");
				continue;
			}

			if (face.ExcludedNeighbours != null && face.ExcludedNeighbours.Contains(definition)) {
				errors.Add("Self-exclusion is not allowed.");
			}
		}

		if (definition.Up.Rotation < 0 || definition.Up.Rotation > 3 || definition.Down.Rotation < 0 || definition.Down.Rotation > 3) {
			errors.Add("Vertical face rotation must be in range [0..3].");
		}

		return errors;
	}
}
