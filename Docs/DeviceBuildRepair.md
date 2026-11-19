# iPhone black-screen build repair

The 1191 device package launched but did not reach gameplay. Device logs first reported a missing `Audio/Placement/Wobble_Normal` resource, then null URP Blitter shaders, followed by repeated `Blitter is already initialized` exceptions. The incremental export's asset inventory contained only three of the eight placement clips despite all eight existing in Assets.

The isolated iOS Library cache was moved aside and rebuilt from source with `-buildTarget iOS` at Editor startup. The clean export passed audio/shader validation and included all eight approved placement clips. No gameplay, sound assets, saved data schema or permanent application identifier was changed.

`IOSDeviceBuilder` now requires iOS as the active target before executeMethod, synchronously refreshes imports, validates every placement clip and both URP Blitter shaders, and uses `CleanBuildCache`. A missing dependency stops export instead of producing another unusable device build.

Use the existing isolated copy and Unity CLI:

```sh
DEVELOPER_DIR=/Applications/Xcode.app/Contents/Developer unity run /path/to/isolated-copy --timeout 1800 --log-file /private/tmp/wobble-ios.log --no-tail -- -buildTarget iOS -executeMethod Wobble.Editor.IOSDeviceBuilder.Build
```

The Xcode export retains the previous temporary signing identity only in `/private/tmp`; source bundle ID remains `com.wobble.game`. The expired development provisioning profile was renewed through Xcode and trusted again on the device. Build **1192** was installed on the connected iPhone 17 Pro.

Physical-device verification using the existing `ProductPlayerReview` harness completed with exit code 0: 32 placements, FLOW, FEVER/WOBBLE, STABILIZED, forced fail, Turkish/English results and fast retry. The save snapshot was restored by the harness. The repaired launch log contains none of the missing-audio/null-shader/Blitter exceptions. Home and gameplay captures were visually inspected. Evidence: `Artifacts/DeviceRepair/`; full local captures/logs: `/private/tmp/wobble-device-repaired-review/` and `/private/tmp/wobble-device-repair-qa.log`.

Harness samples were approximately 33.34 ms/frame, not a 60 FPS certification. Its report retains the older “desktop samples” wording even though this run was on iPhone18,1 / A19 Pro. This repair check verifies startup and Classic gameplay; it is not a new exhaustive four-mode device QA pass.
