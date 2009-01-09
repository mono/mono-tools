#!/bin/bash
#
# Runs all the registered checks
#
function run_checks ()
{
    section_banner Running checks
    for check in $CHECKS_LIST; do
	$check
    done
    message
}

readonly -f run_checks
