#!/bin/bash
# This script checkout submodules from svn

subdir=submodules
file=$subdir/submodules.txt

if [ ! -f $file ]
	then
	echo "Error: file $file not found!";
	exit 1;
fi

cat $file | while read dir	git	location branch; 
do 
	echo "Checkout $git into $subdir/root/$location/$dir ($2)";
	rm -rf $subdir/root/$location/$dir
	git clone $git --branch $2 $subdir/root/$location/$dir
done;
