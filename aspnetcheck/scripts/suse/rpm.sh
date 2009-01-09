#!/bin/bash
#
# Functions to deal with RPMs
#
function rpm_package_exists ()
{
    if [ -z "$1" ]; then
	false
	return
    fi
    rpm -q $1 > /dev/null 2>&1
}

readonly -f rpm_package_exists
