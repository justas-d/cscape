# Basic 317
Implementation of a server responding to clients running a modified 317 revision.

## Changes

* All of the obfuscation methods in the stream class are removed for both write and read operations.
* Player update flag header is a constant 2 byte value, even if the other byte is zero.

* Packets 41, 122, 16, 87, 145 all follow the schema of 
	1. interfaceId
	2. index
	3. itemId

* Walk packets 248, 98, 164, 36 follow the schema of:
	1. Array of local x/y coords of waypoints
	2. 1 Uninterpreted byte