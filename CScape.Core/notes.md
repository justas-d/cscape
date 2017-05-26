# TODO
	* Segregate the Player class into smaller interfaces (maybe)
		* Replace usages of Player with usages of these smaller interfaces.

	* Negative numbers aren't parsed by the command interpreter
	* Invensitage reconnect limbo
	* Separate packet metadata and writing packet data to out stream
	* Replace complicated abstraction hierarchies with a strategy pattern
	* WeakReferences in observatories and sync machines
	* Collision
		* Multi-tile entity movement

	* Different implementations of protocols (377)

	* Packets
		* 85 - sets updRegionPlayerLocalY and updRegionPlayerLocalX

	* updateRegion()
		* 84 - OK ground item update amount
		* 105 - play sound
		* 215 OK 
		* 156 - remove item id from pile
		* 160 - possibly animates an object
		* 147 - transforms player to object
		* 151 - spawn or updates values in an Class30_Sub1 object at the given coords
		* 4 - creates an Animable_Sub3 (whatever that means)
		* 44 OK
		* 101 - same as 151 just less payload args
		* 117 - jesus fucking christ

	* Undocumented incoming packets:
		* 23
		* 35
		* 57
		* 153
		* 228
		* 40
		* 75
		* 156
		* 181
		* 136 (follow prefix?)
	
	* Verify incoming:
		* 192 (item -> object)
		* 25 (item -> floor item)
		* 236 (pickup ground item)
		* 185 (button click)
		* 155 (npc action 1)
		* 129 (bank all)
		* 135 (bank n of item)
		* 117 (bank 5)
		* 73 (trade request)
		* 79 (light item) (ground? inv?)
		* 17 (npc action 2)
		* 21 (npc action 3)
		* 131 (magic on npc)
		* 252 (object action 2)
		* 72 (attack npc)
		* 249 (magic on player)
		* 39 (trade anwser)
		* 43 (bank 10)
		* 237 (magic on item in inventory)
		* 14 (item on player)
		* 18 (npc action 4)
		* 70 (object action 3)
		* 234 (object action 2)
		* 132 (object action 1)
		* 253 (ground item action)
		
		* 188 add friend
		* 133 add ignore
		* 215 del friend
		*  74 del ignore

* Skills:
	* Managed API:
		player.Skills.Farming.Experience { get; set; }
		player.Skills.Farming.Level { get; set; }
		player.Skills.Farming.Boost(int levels);
		player.Skills.Farming.GainExperience(float exp);

