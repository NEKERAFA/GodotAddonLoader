# Class that loads addons written in GDScript
#
# Under MIT License
# Copyright (c) 2023 Rafael Alcalde Azpiazu

extends Node


signal addon_loaded(addonName)

export var addons_path = "user://addons";
export var addon_main_class = "AddonClass"


const RESOURCE_PACK_EXTENSION = ["pck", "zip"]


# Loads the addons contained in "user://addons" folder
func _ready():
	var dir = Directory.new()
	
	# Opens addons folder
	if dir.open(addons_path) == OK:
		# Start to iterate through addons folder
		if dir.list_dir_begin(true, true) == OK:
			var file_name = dir.get_next()
			
			while not file_name.empty():
				load_addon(file_name)
				file_name = dir.get_next()
			
			# Ends addons folder iteration
			dir.list_dir_end()


# Loads the addon
func load_addon(file_name):
	var addon_path = addons_path.plus_file(file_name)
	
	if file_name.get_extension() in RESOURCE_PACK_EXTENSION:
		# Loads the resource pack and instantiate the addon class as GDScript
		if load_addon_resource_pack(addon_path):
			var addon_script = load("res://".plus_file(file_name.get_basename()).plus_file(addon_main_class + ".gd"))
			if addon_script:
				var addon_node = addon_script.new()
				if addon_node:
					add_addon_node(addon_node, file_name)
					emit_signal("addon_loaded", file_name.get_basename())


# Loads the addon resource pack and prints confirmation/error messages
func load_addon_resource_pack(resource_pack_file):
	var resourcePackLoaded = ProjectSettings.load_resource_pack(resource_pack_file)

	if resourcePackLoaded:
		print("Resource pack ", resource_pack_file, " loaded.")
	else:
		print("Error loading resource pack ", resource_pack_file, ".")

	return resourcePackLoaded;


# Adds the addon node as child node using as name the addon name
func add_addon_node(addon_node, addon_name):
	addon_node.name = addon_name.get_basename()
	add_child(addon_node, true)
	print("GDScript add-on ", addon_name.get_basename(), " loaded.")
