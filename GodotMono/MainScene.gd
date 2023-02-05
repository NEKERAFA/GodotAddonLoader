extends Control


func _ready():
	if $AddonsContainer.get_child_count() == 1:
		$AddonsContainer/PlaceholderAddon.text = "Add addons to " + ProjectSettings.globalize_path('user://addons')


func _on_AddonLoader_AddonLoaded(addonName):
	var addon_lbl = Label.new()
	addon_lbl.name = addonName + "lbl"
	addon_lbl.text = " · " + addonName
	$AddonsContainer.add_child(addon_lbl)
