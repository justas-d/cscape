# TODO
	* Separate packet metadata and writing packet data to out stream
	* Packet handler unit tests
	* Replace complicated abstraction hierarchies with a strategy pattern
	* WeakReferences in observatories
	* Only write npc 16383 and player 2047 magic if we will write updates afterward
	* Collision
		* Multi-tile entity movement

	* Skills
		* Test setting dialogID to a level up interface, and instead of setting up the config interface of the interface to print a "Congrats you leveld up" type of deal, we just pushMessage (sendsystemchatmessage) the config message to the client.

	* Rework the syncmachine system to support sync skipping and be more friendly with order in the sync process.
	* Different implementations of protocols (377)
  		
	* Undocumented packets:
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
	
	* Verify:
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