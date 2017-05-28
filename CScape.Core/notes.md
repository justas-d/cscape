# TODO
	* Separate packet metadata and writing packet data to out stream
	* Different implementations of protocols (377)
	* ISyncMachine "does need syncing" property
	* Negative numbers aren't parsed by the command interpreter
	* Invensitage reconnect limbo
	* Replace complicated abstraction hierarchies with a strategy pattern
	* WeakReferences in observatories and sync machines
	* Collision
		* Multi-tile entity movement

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

		
1674 - "defense bonus"
1673 - "attack bonus"

attack
1675 - stab
1676 - slash
1677 - crush
1678 - magic
1679 - range

defense
1680 - stab
1681 - slash
1682 - crush
1683 - magic
1684 - range

1685 - "other bonuses"

1686 - str
1687 - prayer
