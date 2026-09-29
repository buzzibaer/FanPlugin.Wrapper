# Fan Hardware Validation Note Design

## Goal

Keep the README hardware-selection documentation concise while clearly identifying unvalidated hardware variants.

## Scope

Add one neutral note to the existing hardware-selection section. It states that `FanV3` and `Fan20320` have not yet been verified with physical hardware.

The existing `Fan20320` entry remains the configuration reference: it identifies hardware version `20320`, default endpoint `192.168.4.1:20320`, six-digit numeric filename mapping, and the lack of upload support.

## Exclusions

Do not add a separate setup guide, network tutorial, troubleshooting section, or any reference to the implementation's origin.
