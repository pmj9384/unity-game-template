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

## 로딩 씬은 Addressables 프리로드와 함께 둡니다

부팅 로딩 씬에서 Addressables 라벨을 프리로드한 뒤 다음 씬으로 넘어가며, 라벨이 아직 없어도 예외 없이 통과하도록 방어해 두었습니다. 적용 절차와 확장 방침은 [`docs/LOADING-SCENE.md`](docs/LOADING-SCENE.md)에 있습니다.

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

1. 이 리포를 복제하고 Unity 6(6000.x)으로 엽니다.
2. `docs/`의 모듈 문서로 아웃게임·로딩 씬 적용 지점을 확인합니다.
3. 게임 고유 매니저는 `InGameManager`를 상속하고 `"Manager"` 태그를 달아 씬에 두면 `GameManager`가 자동 등록합니다.

## 한계

이 템플릿은 하부 구조만 제공하며, 인게임 플레이 로직·아트·사운드 애셋은 포함하지 않습니다. 광고(`AdsManager`)는 인터페이스 골격만 있어 실제 SDK 연동은 프로젝트에서 붙여야 합니다.
