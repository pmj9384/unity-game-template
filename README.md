# unity-game-template

Unity 6 기반 모바일 게임 시작 템플릿입니다. 여러 프로젝트를 출시하며 매번 다시 짜게 되던 골격 — 매니저 구조, 세이브, 아웃게임 메타, 오브젝트 풀, 데이터 테이블 — 중에서 **검증된 것만 추려 일반화**해 두었습니다. 새 프로젝트는 이 리포를 복제해 게임 고유 로직부터 시작하면 됩니다.

## 매번 새로 짜던 코어를 추출해 만들었습니다

프로젝트마다 GameManager를 다시 세우고, 세이브를 다시 붙이고, 로비·상점·가챠를 다시 만드는 게 반복이라고 느껴, 실제 출시 프로젝트(AnimalBreakOut·TongTongDefence)에서 게임에 종속되지 않는 부분만 떼어내 정리했습니다. 그래서 이 템플릿에는 특정 게임의 규칙이 들어있지 않습니다 — 어떤 장르든 위에 얹을 수 있는 하부 구조만 담았습니다.

## 매니저는 싱글톤 대신 주입 허브로 연결했습니다

전역 접근이 필요할 때마다 싱글톤을 늘리면 의존 방향이 사방으로 흩어져, 대신 `GameManager`를 주입 허브로 두었습니다. 씬의 매니저들은 `"Manager"` 태그를 달고 `InGameManager`를 상속하며, `GameManager`가 `FindGameObjectsWithTag`로 이들을 수집해 등록 순서를 코드에서 명시적으로 제어합니다(`FieldManager` 먼저 등). 매니저 간 참조는 항상 `GameManager.{Manager}`를 경유하므로 하이어라키 배치 순서에 의존하지 않습니다. 순수 C# 매니저(`ObjectPoolManager`)와 MonoBehaviour 매니저는 분리해, 씬이 필요 없는 로직은 씬 밖에서 살게 했습니다.

전역 유일성이 정말 필요한 대상(사운드·데이터·세이브)만 `MonoSingleton`/`PersistentMonoSingleton`으로 남겨, 싱글톤과 주입을 용도에 따라 나눠 씁니다.

## 세이브는 원자적 쓰기와 버전 마이그레이션으로 보호했습니다

저장 도중 앱이 강제 종료돼도 세이브가 반토막 나지 않도록, 임시 파일에 전부 쓴 뒤 `rename`으로 교체하는 원자적 저장을 씁니다. 저장이 중간에 끊겨도 "완전한 옛 파일"이거나 "완전한 새 파일" 둘 중 하나만 남습니다. 세이브 구조에는 버전 필드를 두어, 필드가 바뀐 구버전 세이브도 로드 시점에 마이그레이션됩니다 — 업데이트가 기존 유저의 세이브를 깨지 않게 하기 위해서입니다.

## 아웃게임 메타는 통째로 재사용 가능한 모듈입니다

로비·상점·가챠(1연/10연)·스킨·하단 네비게이션·스태미나까지, 하이퍼캐주얼/캐주얼 모바일 게임이 공통으로 쓰는 아웃게임 흐름을 하나의 모듈로 묶었습니다. 화면 전환은 `OutGameUIManager`가 상태 기반으로 관리하고, 재화·구매는 즉시 저장돼 강제 종료 롤백을 막습니다. 자세한 구조와 확장 지점은 [`docs/OUTGAME-MODULE.md`](docs/OUTGAME-MODULE.md)에 정리해 두었습니다.

## 데이터는 CSV 테이블로 코드와 분리했습니다

레벨·스킨 같은 수치 데이터는 `DataTableManager`가 CSV에서 읽어 들여, 밸런스 조정을 코드 수정 없이 하도록 했습니다.

파싱은 `Plugins/CsvHelper.dll` 하나로 통일합니다 — 아웃게임 테이블(`DataTable` 경유)만이 아니라, 게임 쪽 규칙 테이블을 만들 때도 같은 DLL을 씁니다. WarTableSimulator에서 확립한 패턴은 이렇습니다: 게임 규칙 테이블은 별도 asmdef(`Game.Core`, `noEngineReferences`)의 순수 파서가 `ClassMap` 명시 매핑 + 범위 검증(행 번호 보고)으로 읽고, 실패는 `FormatException` 하나로 감싸 호출측이 라이브러리를 모르게 하며, EditMode 테스트가 실제 CSV를 스펙과 전건 대조합니다. 자작 CSV 파서는 만들지 않습니다.

## 로딩 씬은 Addressables 프리로드와 함께 둡니다

부팅 로딩 씬에서 Addressables 라벨을 프리로드한 뒤 다음 씬으로 넘어가며, 라벨이 아직 없어도 예외 없이 통과하도록 방어해 두었습니다. 적용 절차와 확장 방침은 [`docs/LOADING-SCENE.md`](docs/LOADING-SCENE.md)에 있습니다.

## 2D·3D를 한 리포로 굴립니다

템플릿의 본체(매니저 골격·세이브·아웃게임 메타·오브젝트 풀)는 2D든 3D든 완전히 같아서, 리포를
나누지 않고 **차원 전용 패키지만 빼두었습니다.** 나누면 세이브 하나 고칠 때 두 리포에 반영해야 해서,
"매번 다시 짜지 않는다"는 이 템플릿의 존재 이유가 절반 무너집니다.

새 프로젝트를 시작할 때 아래 표를 보고 필요한 것만 추가하면 됩니다.

**manifest에 넣는 기준은 "템플릿 코드가 실제로 참조하는가" 하나입니다.** 없으면 컴파일이 안 되는 것만
넣습니다(Addressables·Newtonsoft·TMP·Input System·AdMob). 게임이 쓸지 말지 모르는 건 안 넣습니다 —
NavMesh나 Timeline이 그렇습니다. 넣어두면 편해 보이지만, 안 쓰는 패키지는 임포트 시간과 빌드에만
얹히고 "이건 왜 있지"를 매번 되묻게 됩니다.

| 만들 게임 | 추가할 것 |
|---|---|
| 3D | 필요하면 `com.unity.ai.navigation` (NavMesh를 쓸 때만) |
| 2D | `com.unity.feature.2d` + 물리 설정·카메라 투영(Orthographic)·URP 렌더러(2D Renderer) 교체 |

`test-framework`는 참조하는 테스트가 아직 없지만 남겨둡니다 — 나중에 붙이려면 어셈블리 정의부터
다시 잡아야 해서, 빈 슬롯을 두는 편이 쌉니다.

`ProjectSettings`와 `Assets/Settings`(URP 렌더러·볼륨 프로파일)가 3D 기준으로 들어 있습니다.

## 에디터 도구

메뉴 `Tools/` 아래에 있습니다. 새 프로젝트에서 고칠 곳은 각 파일 맨 위 상수 블록뿐입니다.

- **Build/Android APK · AAB** (`BuildScript`) — 패키지명·제품명·화면 방향·아키텍처를 빌드마다 강제해
  "설정 빠뜨린 빌드"가 안 나오게 합니다. APK는 로컬 배포·테스트용, AAB는 스토어 제출용입니다.
  씬 목록은 Build Settings를 그대로 읽습니다(복사본을 들고 있다가 씬 추가를 놓치는 사고 방지).
- **Addressables/Register Folder** (`AddressablesFolderRegistrar`) — 폴더 안 에셋을 그룹에 일괄 등록합니다.
  주소가 `{접두사}/{파일명}`이라 코드의 키와 1:1로 맞습니다.
- **Cheat 창** (`CheatWindow`) — 재화 증감 등 Play 중 상태를 건드립니다.
- **Font 일괄 적용** (`FontApplier`)

## 구조

```
Assets/Scripts/
  Core/
    Managers/        GameManager(주입 허브)·InGameManager·GameDataManager·ObjectPoolManager·SoundManager·AdsManager·StaminaSystem
    Managers/SaveLoad/   원자적 저장 + 버전 마이그레이션 (SaveData·PlayerAccountData·SkinUserData·StaminaSystemSave)
    Singleton/       MonoSingleton·PersistentMonoSingleton (용도 한정 싱글톤)
    Interfaces/      IManager·IObjectPoolable
  Data/              DataTableManager (CSV 구동)
  OutGame/           로비·상점·가챠·스킨·네비 — 재사용 메타 모듈
  Defines/           Enums
docs/                모듈별 적용·확장 문서
```

## 시작하기

1. 이 리포를 복제하고 Unity 6(6000.3.x)으로 엽니다. 컴파일이 붙는 것까지 확인된 상태입니다.
2. `Assets/Editor/BuildScript.cs` 맨 위 상수 블록에서 패키지명·제품명·화면 방향을 바꿉니다.
3. 2D 게임이면 `com.unity.feature.2d`를 추가합니다(3D는 URP 템플릿 기준이라 그대로 씁니다).
4. `docs/`의 모듈 문서로 아웃게임·로딩 씬 적용 지점을 확인합니다.
5. 게임 고유 매니저는 `InGameManager`를 상속하고 `"Manager"` 태그를 달아 씬에 두면 `GameManager`가 자동 등록합니다.

검증은 에디터를 안 열고도 됩니다.

```
unity projects verify .    # meta 누락·무결성
unity run .                # 임포트 + 컴파일
unity test .               # EditMode/PlayMode
```

에디터를 열어 둔 상태에서는 위 배치 명령이 거부되는 대신, 프로젝트에 `unity pipeline install`로 넣은 파이프라인 서버에 붙습니다.
`unity status --json`에 ready가 뜨면 `unity command run_tests --mode EditMode --json`으로 테스트를 돌리고,
`recompile`·`get_console_logs`·`get_scene_hierarchy`·`screenshot` 같은 명령(`unity command --list`, 142개)으로 에디터를 읽고 조작할 수 있습니다.
AI 도구 연결(MCP)은 같은 서버를 쓰므로 따로 등록하지 않아도 됩니다.

## 한계

이 템플릿은 하부 구조만 제공하며, 인게임 플레이 로직·아트·사운드 애셋은 포함하지 않습니다.
광고(`AdsManager`)는 Google Mobile Ads로 리워드·배너를 띄우는 코드까지 있고, 광고 단위 ID 두 개(`REPLACE_WITH_*`)와 앱 ID 에셋(`Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset`)만 프로젝트에서 채웁니다. 배너를 어디서 띄울지는 게임이 정합니다(예: 로비 Open에서 `ShowBanner`, 인게임 진입 때 `HideBanner`).
