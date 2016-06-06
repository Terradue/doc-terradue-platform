#!/bin/bash
# This script links submodules

file=submodules.txt

if [ $# -eq 0 ]
  then
    echo "Usage : update-subs.sh <path to directory containing submodules>";
    exit 1;
fi

if [ ! -f $file ]
	then
	echo "Error: file $file not found!";
	exit 1;
fi

cat $file | while read dir	svn	location; 
do 
	git=`echo "$svn" | awk -F"/" '{print $NF}'`
	gitdir="${git%.*}"
	path="$1/$gitdir";
	rm -f $dir
	echo "ln -fs $path $dir"; 
	ln -fs $path $dir; 
done;
