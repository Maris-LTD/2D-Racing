# PHÂN TÍCH HIỆU NĂNG - DỰ ÁN 2D RACING

## 🔴 VẤN ĐỀ NGHIÊM TRỌNG (Cần ưu tiên sửa)

### 1. **SplineTrackData.cs** - Tạo NativeSpline mỗi frame
**Vị trí:** `GetNearestPoint()`, `GetProgress()`
**Vấn đề:** 
- Mỗi lần gọi `GetNearestPoint()` hoặc `GetProgress()` đều tạo mới `NativeSpline` với `using var`
- Với nhiều AI car, điều này xảy ra hàng trăm lần mỗi giây
- `NativeSpline` là struct lớn, việc tạo mới liên tục gây GC pressure

**Ảnh hưởng:** 
- GC allocations cao
- Hiệu năng giảm đáng kể với nhiều AI car

**Giải pháp:** Cache `NativeSpline` hoặc tái sử dụng

---

### 2. **AICarInputStrategy.cs** - Physics.Raycast mỗi frame
**Vị trí:** `DetectAndAvoidObstacles()` (dòng 209)
**Vấn đề:**
- `Physics.Raycast()` được gọi mỗi 0.2 giây cho mỗi AI car
- Với 10 AI cars = 50 raycasts/giây
- Raycast là operation tốn kém

**Ảnh hưởng:**
- Tăng tải cho Physics system
- Có thể gây frame drops

**Giải pháp:** 
- Sử dụng Physics.OverlapSphere thay vì Raycast
- Hoặc cache kết quả lâu hơn
- Sử dụng LayerMask để giới hạn objects được check

---

### 3. **TrackLoader.cs** - Blocking main thread
**Vị trí:** `LoadTrack()` (dòng 16)
**Vấn đề:**
- `handle.WaitForCompletion()` block main thread cho đến khi load xong
- Gây freeze UI trong lúc load track

**Ảnh hưởng:**
- Game bị đơ khi load track
- Trải nghiệm người dùng kém

**Giải pháp:** 
- Sử dụng async/await hoặc coroutine
- Hiển thị loading screen

---

### 4. **Observer.cs** - Type lookup mỗi lần Notify
**Vị trí:** `Notify<T>()`, `AddObserver<T>()`, `RemoveObserver<T>()`
**Vấn đề:**
- `typeof(T)` được gọi mỗi lần, tạo allocation
- Dictionary lookup với Type key có thể chậm

**Ảnh hưởng:**
- Với nhiều events được gửi mỗi frame, overhead tích lũy
- GC allocations từ typeof()

**Giải pháp:**
- Cache Type trong static field hoặc sử dụng TypeId
- Sử dụng generic type caching

---

## 🟡 VẤN ĐỀ TRUNG BÌNH

### 5. **MapController.cs** - ToList() tạo list mới
**Vị trí:** `GetSpawnPoints()` (dòng 44)
**Vấn đề:**
- `_spawnPoints.ToList()` tạo list mới mỗi lần gọi
- Allocation không cần thiết

**Ảnh hưởng:**
- GC allocations
- Nếu được gọi thường xuyên sẽ tích lũy

**Giải pháp:**
- Trả về `IReadOnlyList` hoặc array
- Cache list nếu không thay đổi

---

### 6. **CarInputUI.cs** - Update() check keyboard mỗi frame
**Vị trí:** `Update()` (dòng 42-46)
**Vấn đề:**
- `Input.GetKey()` được gọi mỗi frame
- Có thể sử dụng Input System events thay vì polling

**Ảnh hưởng:**
- Overhead nhỏ nhưng không cần thiết
- Polling input mỗi frame

**Giải pháp:**
- Sử dụng Unity Input System events
- Hoặc chỉ check khi có thay đổi

---

### 7. **InGameUI.cs** - String concatenation
**Vị trí:** `ShowLevelResult()` (dòng 94-101)
**Vấn đề:**
- Nhiều string concatenations tạo nhiều string objects tạm thời
- GC allocations

**Ảnh hưởng:**
- GC spike khi hiển thị kết quả
- Không nghiêm trọng nhưng có thể tối ưu

**Giải pháp:**
- Sử dụng `StringBuilder` hoặc string interpolation với format

---

### 8. **CheckpointTracker.cs** - TryGetComponent trong OnTriggerEnter
**Vị trí:** `OnTriggerEnter()` (dòng 43)
**Vấn đề:**
- `TryGetComponent<Checkpoint>()` được gọi mỗi lần trigger
- Component lookup có overhead

**Ảnh hưởng:**
- Nếu có nhiều triggers xảy ra đồng thời
- Có thể cache component reference

**Giải pháp:**
- Cache Checkpoint component nếu có thể
- Hoặc sử dụng tag/layer để filter trước

---

### 9. **CarController.cs** - Foreach trong Update/FixedUpdate
**Vị trí:** `Update()`, `FixedUpdate()` (dòng 82-93)
**Vấn đề:**
- Foreach loops có thể tạo enumerator allocations (tùy Unity version)
- Với nhiều modules, overhead tích lũy

**Ảnh hưởng:**
- GC allocations từ enumerator
- Overhead nhỏ nhưng có thể tối ưu

**Giải pháp:**
- Sử dụng for loop với index
- Hoặc đảm bảo Unity version không tạo allocations

---

### 10. **AICarInputStrategy.cs** - Nhiều tính toán trong UpdateInput
**Vị trí:** `UpdateInput()` (dòng 63-141)
**Vấn đề:**
- Nhiều phép tính toán phức tạp mỗi frame cho mỗi AI car
- Vector3 operations, quaternion calculations

**Ảnh hưởng:**
- CPU usage cao với nhiều AI cars
- Có thể tối ưu bằng cách giảm tần suất update

**Giải pháp:**
- Đã có caching tốt (curvature, obstacle, path)
- Có thể tăng interval cho một số calculations

---

## 🟢 VẤN ĐỀ NHỎ (Có thể cải thiện)

### 11. **CarMovementController.cs** - GetComponent trong OnCarInit
**Vị trí:** `OnCarInit()` (dòng 22)
**Vấn đề:**
- `GetComponent<Rigidbody>()` được gọi mỗi lần init
- Nên cache từ trước

**Giải pháp:**
- Cache Rigidbody reference trong CarController

---

### 12. **CarInputModule.cs** - Event broadcasting mỗi frame
**Vị trí:** `OnUpdate()` (dòng 57)
**Vấn đề:**
- Event được broadcast mỗi frame nếu có thay đổi
- Với nhiều cars, nhiều events được gửi

**Giải pháp:**
- Đã có check `InputsEqual()` - tốt
- Có thể thêm throttling nếu cần

---

### 13. **MapController.cs** - GetComponentsInChildren trong Awake
**Vị trí:** `Awake()` (dòng 28)
**Vấn đề:**
- `GetComponentsInChildren<CarSpawnPoint>()` được gọi nếu array null
- Nên set trong Inspector

**Giải pháp:**
- Đảm bảo set trong Inspector
- Hoặc cache kết quả

---

## 📊 TỔNG KẾT

### Mức độ ưu tiên sửa:
1. **Cao:** SplineTrackData NativeSpline, AICarInputStrategy Raycast, TrackLoader blocking
2. **Trung bình:** MapController ToList, CarInputUI Update, InGameUI string concat
3. **Thấp:** Các vấn đề nhỏ khác

### Ước tính cải thiện:
- **SplineTrackData:** Giảm 30-50% GC allocations
- **AICarInputStrategy:** Giảm 20-30% Physics overhead
- **TrackLoader:** Loại bỏ freeze khi load
- **Tổng thể:** Cải thiện 15-25% FPS với nhiều AI cars

