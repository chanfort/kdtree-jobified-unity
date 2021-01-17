# Jobified KD Tree stress tests for Unity

This repository contains jobified version of KDTree which is compatible to run with Unity DOTS jobs. The `ArrayStressTests` scene includes stress tests to compare performance while using build-in arrays and native arrays in regular way (inline) and in jobs. Use `A`, `B`, `C`, `D`, `E` and `F` keys in order switch between different modes when running the scene. The `KDTreeTests` scene allows developers to test correctness for KDTree regular and jobified implementations as well as performance differences while running them. The following performance modes can be tested in `KDTreeTests` scene by pressing these keys:

`A` - Regular KDTree test.
`B` - Jobified KDTree while running dirrectly from the man thread (non-jobified test).
`C` - IJobParallelFor jobified run.
`D` - IJobParallelFor jobified run with Burst enabled.

Performance differences can be observed through FPS counter.
