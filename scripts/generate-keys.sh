#!/bin/bash

#
# Generates crypto keys for use in the login protocol.
# Do NOT share the private key, only the public key needs to be shipped with the client.
#

SIZE=2048
KEYNAME=cryptokey-private.pem
PUBKEYNAME=cryptokey-public.der

openssl genrsa -out $KEYNAME $SIZE
openssl rsa -in $KEYNAME -pubout -outform DER -out $PUBKEYNAME

TDIR=../CScape/Meta

if [[ -z $1 ]] ; then
	rm $TDIR/$KEYNAME
	cp $KEYNAME $TDIR
fi
