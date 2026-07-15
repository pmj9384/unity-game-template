# 아웃게임 모듈 (로비·샵·스킨·가챠·세이브) 이식 가이드

> TongTongDefence 이식(2026-07-15)에서 검증된 절차. 실측: 순수 이식 반나절, 게임 연결까지 1일.
> 모듈 구성: UISystem 프레임워크 + OutGame 화면/팝업/위젯 + GachaService + 세이브 체인(GameDataManager/
> SaveLoadSystem/PlayerAccountData/SkinUserData/StaminaSystem) + Data 테이블 + LobbyScene.

## 이식 절차 (순서 중요)

### 1. 파일 복사 — 반드시 .meta째 (GUID 보존)
씬·프리팹이 스크립트를 GUID로 참조하므로, **meta 없이 복사하면 씬 참조가 전부 끊긴다.**
```
Scripts/{OutGame, UISystem, Data}  Scripts/Core/{Singleton, Managers(세이브 체인)}
Scenes/LobbyScene.unity  Prefabs/{GachaResultItemUI, SettingsPanel, UI/SafeAreaPanel, UI/SkinItemUI}
Resources/Tables/SkinData.csv  Resources/Sprites/UI/card_panel.png  Sprites/Circle.png
Plugins/CsvHelper/  Editor/GameDataManagerEditor.cs(코인 치트)
```

### 2. 대상 프로젝트에 이미 있는 파일 = GUID 재매핑
같은 경로 파일이 이미 있으면(다른 GUID) 복사하지 말고, **가져온 씬/프리팹 안의 GUID를 치환**한다.
TongTong 실측 재매핑 4종: 폰트(Kostar SDF 2) / SaveLoadSystem.cs / SoundManager.cs / SafeAreaCanvas.cs
```python
# 씬/프리팹 텍스트에서 s.replace(원본guid, 대상guid)
```

### 3. 필수 사전 조건 (하나라도 빠지면 부팅 NRE/예외)
- [ ] `Packages/manifest.json`에 `com.unity.nuget.newtonsoft-json` (세이브 JSON)
- [ ] **`Resources/Tables/SkinData.csv` 존재** — 부팅 체인이 기본 스킨 장착 때 로드. 없으면 NRE (실사례)
- [ ] 태그 `UIManager` 정의 (OutGameManager가 FindGameObjectWithTag)
- [ ] 게임 씬 이름 — LobbyScreen의 `SceneManager.LoadScene("SampleScene")` 대상 확인
- [ ] 빌드 씬 목록: LobbyScene=0, 게임씬=1

### 4. 어댑터 지점 (대상 프로젝트마다 다른 곳)
- **사운드 API**: 모듈은 `SoundManager.Instance.bgmVolume/SetBgmVolume`(이 템플릿 기준)을 부름.
  대상의 SoundManager 시그니처가 다르면 Settings 패널 2곳 수정 (TongTong: PascalCase 어댑터 2줄)
- **UIElementEnums**: 인게임 SettingsPanel을 쓰면 enum 항목 + GameUIManager uiElements 등록 필요
- **레거시 오디오 프리팹**: LobbyScene의 SoundManager/AudioSourcePlayer 인스턴스는 이 템플릿 사운드
  스택용 — 대상이 다른 사운드면 씬에서 제거 후 대상 것 배치
- **코인 획득처**: 게임 쪽에서 `PlayerAccountData.AddCoins()` 배선 (TongTong: 게임오버 시 점수÷5)
- **스킨 적용점**: 장착 id → 대상 게임 캐릭터 반영 컴포넌트는 게임마다 신규
  (예: TongTongDefence `PilotSkinApplier` — 3파츠 SpriteRenderer 교체, 폴백 포함)

### 5. 스킨 스프라이트 규약
`Resources/Sprites/Skins/{SkinId}.png` = 아이콘 (SkinItemUI·가챠 팝업·장착 표시 공용).
Addressables로 옮기려면 로딩 지점을 하나로 모아 치환 (TongTong: `SkinSprites.Load` 단일 지점 +
`SkinAddressablesSetup` 에디터 일괄 등록 — Addressables 2.9.0, 참고: TongTongDefence 리포)

### 6. 검증 체크리스트
- [ ] 로비 부팅 (NRE 없이), 코인 0/스태미나 5·5
- [ ] Play → 스태미나 소모 → 게임 진입 / 게임 → 로비 복귀
- [ ] 코인 치트(+1000) → 뽑기 1/10회 → NEW 뱃지·차감·보유 반영 → 재시작 유지
- [ ] 스킨 장착 → 게임 캐릭터 반영

## 함정 기록 (실사례)
- **meta 없이 복사 → 씬 missing script 전멸** (그래서 1번이 철칙)
- SkinData.csv 미배치 → GameDataManager 부팅 NRE
- 씬을 코드/YAML로 고친 뒤 **에디터 리로드 안 하면** 메모리-디스크 불일치로 유령 NRE
- 에디터 자동화에서 `EditorApplication.delayCall` 금지 — 에디터가 백그라운드면 영영 안 돈다
