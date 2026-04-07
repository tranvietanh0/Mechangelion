# Báo cáo nghiên cứu: Tài liệu nội bộ cho dự án Unity

_Thực hiện: 2026-04-07_

## Tóm tắt
- Có 1 mẫu chung khá rõ giữa các nguồn mạnh: dùng docs-as-code để tài liệu đi cùng vòng đời code; dùng khung kiến trúc có cấu trúc như C4 + arc42; và ghi lại quyết định kiến trúc riêng thay vì trộn lẫn vào mô tả trạng thái hiện tại [1][2][3].
- Với Unity, tài liệu phải bao phủ cả phần “không nằm hết trong code”: scene, prefab, ScriptableObject, dữ liệu Inspector/serialization, asmdef, package manifest, và các ràng buộc Editor/runtime. Nếu không, tài liệu kiến trúc sẽ nhanh lỗi thời dù C# vẫn đúng [4][5].

## Nên tài liệu hóa gì
- `Current state / as-built`: sơ đồ context, module/assembly boundaries, scene flow, asset ownership, dependency rules, runtime bootstrap, data flow quan trọng, package/SDK đang dùng [2][3][5].
- `Standards`: quy ước đặt tên, folder/package layout, asmdef reference rules, dependency inversion, scene/prefab authoring rules, cách dùng ScriptableObject, Addressables, serialization-safe patterns [3][4][5].
- `Decisions`: ADR cho lựa chọn khó đổi như DI framework, state machine model, save/load format, event bus, scene loading model, package policy [3].
- `Operational docs`: cách thêm scene mới, thêm assembly mới, checklist review, migration notes, known risks/tech debt [1][3].

## Phân biệt tài liệu hiện trạng với kiến trúc đề xuất
- Tách file và nhãn rõ ràng: `current/` hoặc `as-built/` cho hệ thống đang chạy; `proposed/` hoặc `target/` cho thiết kế tương lai. Không trộn trong cùng sơ đồ mà không dán nhãn [2][3].
- Trong mỗi tài liệu, thêm `Status`, `Last verified`, `Source of truth`, `Applies to commit/package version` để người đọc biết đó là mô tả đã xác minh hay ý tưởng đang chờ làm [1][3].
- Dùng ADR để nối hai thế giới: tài liệu current-state mô tả hệ thống đang có; ADR giải thích tại sao đổi; tài liệu proposed mô tả đích cần đạt [3].
- Với sơ đồ C4, giữ một bộ diagram phản ánh hệ thống thật hiện tại; nếu cần target architecture thì tạo bộ diagram riêng thay vì annotate chồng chéo [2].

## Cách giữ tài liệu gần code
- Dùng docs-as-code: Markdown trong repo, review cùng PR, cùng issue tracker, có thể chặn merge nếu thay đổi kiến trúc/standard mà thiếu cập nhật docs [1].
- Đặt README cạnh từng module Unity chính, ví dụ `Assets/Scripts/<Module>/README.md`, nêu trách nhiệm module, public entry points, dependencies, scenes/prefabs/ScriptableObjects liên quan.
- Đặt tài liệu mức hệ thống ở `docs/architecture/` hoặc `plans/.../research/`, nhưng giữ tài liệu mức module cạnh asmdef hoặc package tương ứng để drift khó xảy ra [1][5].
- Dùng “review triggers”: đổi `*.asmdef`, `Packages/manifest.json`, bootstrap scene, shared ScriptableObject schema, serialized fields công khai, hay editor tooling => bắt buộc chạm docs liên quan.
- Nếu có CI, thêm check nhẹ: file touched thuộc nhóm kiến trúc mà không đổi `docs/` hoặc README module thì cảnh báo, không cần cố auto-generate kiến trúc hoàn toàn.

## Bẫy đặc thù Unity
- `Serialized state != code`: nhiều hành vi thật nằm trong scene, prefab overrides, Inspector references, và asset data; chỉ đọc C# sẽ thiếu bức tranh hệ thống [4][5].
- `ScriptableObject` là asset độc lập, thường chứa data chia sẻ và đôi khi cả behavior; phải ghi rõ ai sở hữu asset, runtime đọc/ghi ở đâu, và asset nào là cấu hình nền tảng [5].
- Đổi dữ liệu ScriptableObject bằng script trong Editor có thể không được lưu nếu không đánh dấu dirty; tài liệu quy trình authoring nên nêu rõ để tránh “doc đúng, asset sai” [5].
- `Asmdef` là ranh giới kiến trúc thực tế trong Unity; nếu docs nói module độc lập nhưng asmdef tham chiếu chéo bừa bãi thì docs vô nghĩa [4].
- Serialization rules và performance ảnh hưởng trực tiếp model dữ liệu; nếu standard docs không nêu field nào được serialize, versioning/migration ra sao, team dễ tạo coupling ẩn hoặc mất dữ liệu [4].
- Tránh viết docs chỉ từ hierarchy tĩnh; Unity có thêm dependency qua addressable keys, inspector object refs, menu-created assets, codegen, và editor scripts.

## Khuyến nghị ngắn gọn cho team nội bộ
- Dùng `C4` cho sơ đồ cấp hệ thống/module, `arc42` làm checklist nội dung cần có, và `ADR` cho quyết định thay đổi kiến trúc [2][3].
- Chuẩn hóa 3 lớp tài liệu: `docs/architecture/current`, `docs/architecture/proposed`, `docs/standards`; mỗi module có README cạnh code.
- Mỗi tài liệu kiến trúc nên có 5 trường bắt buộc: `Status`, `Owner`, `Last verified`, `Evidence` (scene/asmdef/package/commit), `Related ADRs`.
- Với Unity, tối thiểu phải có bảng inventory cho `Scenes`, `Prefabs`, `ScriptableObjects`, `Asmdefs`, `Packages`, `Editor tools` và chủ sở hữu của từng nhóm.

## Nguồn
- [1] Write the Docs, “Docs as Code” — tài liệu nên dùng issue tracker, version control, plain text markup, code review, automated tests: https://www.writethedocs.org/guide/docs-as-code/
- [2] Simon Brown, “The C4 model for visualising software architecture” — kiến trúc nên thể hiện theo các mức system/container/component/code: https://c4model.com/
- [3] arc42, “Template Overview” — checklist tài liệu kiến trúc gồm goals, constraints, context, building blocks, runtime, deployment, cross-cutting concepts, decisions, risks, glossary: https://arc42.org/overview
- [4] Unity Manual, “Script serialization” và “Organizing scripts into assemblies” — serialization ảnh hưởng hiệu năng và dữ liệu; asmdef giúp làm rõ kiến trúc và dependency boundaries: https://docs.unity3d.com/Manual/script-serialization.html ; https://docs.unity3d.com/Manual/assembly-definition-files.html
- [5] Unity Manual, “ScriptableObject” — ScriptableObject là asset độc lập, tham chiếu qua Inspector, có thể lưu data dùng chung, và có lưu ý riêng khi sửa trong Editor: https://docs.unity3d.com/Manual/class-ScriptableObject.html

## Bước tiếp theo
- Chuyển các khuyến nghị trên thành template tài liệu cho repo này: `architecture-current.md`, `architecture-proposed.md`, `code-standards.md`, `module-readme.md`.
- Thêm checklist PR: nếu đổi scene/prefab/ScriptableObject/asmdef/package mà không cập nhật docs liên quan thì reviewer phải chặn.
