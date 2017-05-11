# TODO
	* Move the command system out to a different project.
	* Different implementations of protocols (377)
	* Test setting dialogID to a level up interface, and instead of setting up the config interface of the interface to print a "Congrats you leveld up" type of deal, we just pushMessage (sendsystemchatmessage) the config message to the client.
  		
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

	* Action:
		* Any type of walking.
	
		* 139 and 128 follow
	
	* Anticheat:
		* 189
		* 230
		* 152
		* 200
		* 85
	
	* Verify:
		* 192 (item -> object)
		* 25 (item -> floor item)
		* 236 (pickup ground item)
		* 122 (item option 1)
		* 185 (button click)
		* 155 (npc action 1)
		* 129 (bank all)
		* 16  (item alt opt 2)
		* 135 (bank n of item)
		* 53 (item on item)
		* 87 (drop item)
		* 117 (bank 5)
		* 73 (trade request)
		* 79 (light item) (ground? inv?)
		* 145 (unequip? alt item action?)
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
		* 41 (equip item)
		* 18 (npc action 4)
		* 70 (object action 3)
		* 234 (object action 2)
		* 132 (object action 1)
		* 253 (ground item action)
		
		* 188 add friend
		* 133 add ignore
		* 215 del friend
		*  74 del ignore


# Interface (and container) system:
	* Goals:
		* Support all types of interfaces.
			* Main (big, on-screen ones)
			* Sidebar
			* Input (input boxes for numbers and strings etc)
					* With a callback, which is triggered when the input is filled.
	
		* Make interface lifetime management intuitive.
			* var clueReward = new ClueScrollRewardInterface(player);
			  
			  clueReward.Items.Add(new ItemStack(5, 1) ...); // sched update
			  ...
			  player.Interfaces.Show(clueReward);
			  
			* var invSidebar = new InventorySidebar(player);
			
			  // map db types to server types
			  invSidebar.Items = _model.Items; // sched update
			  
			  player.Interfaces.Show(invSidebar);
			  
			* var dialog = new CharacterDialog(player);
			  
			  // config is implementation dependant
			  dialog.Config.SetHead(player, HeadAnimation.Invariant); // sched update
			  dialog.Config.SetText("Hello world!"); // sched update
			  
			  player.Interfaces.Show(invSidebar);
			  
			* player.Interfaces.CloseMain();
			* player.Interfaces.CloseSidebar();
			
			* player.Interfaces.Sidebar; // all active sidebar interfaces
			* player.Interfaces.Main; // active main interface
			* player.Interfaces.Input; // active input interface
			
		* Item adding/removing should handle:
			* Item amounts
			* Empty items,
			* Item amount overflows
			* Container overcapacity
			
		* Interfaces should expose an Dispose method.
			* Called when the interface is closed.
			  This should be called if the interface is closed as a result of something else happening. (teleport, moves, change interface, change state, death etc)
			  Direct close interface requests (some sort of close interface packet from the client) should never happen.
			  
		* The "Item" property manages handled addition and removal from the underlying item containment structure, which is exposed publicly.
		
		* Once we assign a IItemProvider to an interface, that interface owns the item provider.
			* No two interfaces can own the same IItemProvider.
			* IItemProvider should only be changed by the owning interface, if there is one.
			  
	* Implementation:
	
		* Per-player central interface manager.
	
		* Per-player interface sync machine which handles dispatch of all interface updates.
			* Interfaces are responsible for scheduling update messages themselves as soon as something update-worthy has occured.
			* Interface manager pushes/removes syncable interfaces to this sync machine.
		
		* Interface:
			* InterfaceId;
			* IEnumerable<IPacket> GetUpdates();
			
		* ItemProviderChangeInfo
			* Struct
			* Describes how to manipulate the underlying item provider;
			
			* Impl:
				* IsValid
					* Whether this operation can be carried out. (set if input is invalid, empty, null, container is full etc).

				* Index
					* At which index in the underlying item array the operation must be carried out.
				* AmountDelta
					* The difference in amount that the operation will apply.
				* ItemDefinitionId
				* OverflowAmount
					* The amount of the item that, due to maximum stack amounts, or because the contains is full, could not have been added
					  to the container.
								
		* Container:
			* Manages item look up, item adding and removing all on the underlying item provider.
		
			*Impl:
			
				* CalcChangeInfo(id, amnt);
				* ExecuteChangeInfo(ItemProviderChangeInfo)
					* Virtual
					* Changes the underlying item container as describe in the change info, without taking into account the OverflowAmount.
					  Does nothing on invalid info.
					  
				* Contains (id) (returns amount or 0 if item doesn't exist in the provider)
				* Size (maximum capacity)
				* Count (the number of items that aren't empty (id = 0 || amount <= 0)
				* Provider (returns the underlying IItemProvider)
				
		* Config:
			* Manages the details of the interface.
			* This is interface implementation specific and is not required to exist.
			

		
========== Initializers/Setters ==========	

Sidebar:
	* tabInterfaceIDs 
		* 71
	* sidebarOverlayInterfaceId 
		* 248 - "inteface with inventory" 
		* 142 - "opens inventory")
			
Chat:
	* dialogID 
		* 218 - "chat box interface"
	* backDialogID
		* 164 - "show back dialog"

openInterfaceID - main
		* 248 - "inteface with inventory" 
		* 97  - "show interface"
		
inputDialogState - input
		* 27 == 1, "enter emount"
		* 187 == 2, "enter name"

========= Overwrites ===========

tabInterfaceIDs
	* itself, 71 (-1 | short.MaxValue)

sidebarOverlayInterfaceId
	* 164
	* 219
	* 97
	* clearTopInterfaces
	* 248 - itself
	* 142 - itself
		
dialogID
	* 218 - itself, -1
	
backDialogID
	* 164 - itself
	* 219
	* 97
	* clearTopInterfaces
	* 248
	* 142
	
openInterfaceID
	* 164
	* 219 - itself
	* 97 - itself
	* clearTopInterfaces
	* 248 - itself
	* 142
	
	* 176 (welcome screen)
	
inputDialogState
	* Action
	* 248
	* 142
	* 97
	* 219

clearTopInterfaces()
	* 176 (welcome screen)
	* Report button and openInterfaceID == -1
	

	
----------------------------------------
	
SidebarOverlay - sidebarOverlayInterfaceId
	* ChatOverlay
	* Main

ChatOverlay - backDialogID
	* Main
	* SidebarOverlay

Main - openInterfaceID
	* ChatOverlay
	* SidebarOverlay
	
Sidebar - tabInterfaceIDs
Chat - dialogID



Input - inputDialogState
	* Any action
	* Main
	* SidebarOverlay

Packet 248:
	Open interface 1 and open interface 2 as sidebar overlay.
	

=== FINAL ===

Main
	* Big fat interfaces
	* Chat overlays
	* Sidebar overlays(unless 248)
	
Sidebar[]
Chat
Input
	

