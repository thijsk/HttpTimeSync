## Synopsis

This is a service that syncs to local clock by getting the current time from the HTTP Date: header of serveral sites and calculating the offset to local.

## Motivation

The precision is about .5 seconds, if you need more precision use regalur NTP. 
This service is meant to be used on networks where UDP port 123 is firewalled and an alternative time sync is needed.

## Installation

Build using VS2013 / Wix3.9
Run the setup from the msi to install and run the service.
