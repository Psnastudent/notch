# Performance Strategy

- The idle shell must not use a render loop, high-frequency timer, or continuous idle polling.
- Settings and optional services load asynchronously and lazily.
- UI updates occur on user interaction or Windows events; system metrics sample only while their card is visible.
- Low Resource and Gaming modes suspend decorative effects and nonessential modules.
- Icons and artwork use bounded caches and release unused image resources.
- Benchmarks will record CPU, working set, private bytes, startup, shutdown, expanded state, and a two-hour memory-stability run. No measurements have been taken yet.
