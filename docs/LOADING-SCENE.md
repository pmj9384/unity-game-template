# 아웃게임 로딩 씬 모듈 (2026-08-03, TongTongDefence 역이식)

부팅 진입 씬(빌드 0번): 스플래시 + Addressables 라벨 프리로드(진행바 실배선) + 실패 폴백 → 다음 씬.
AnimalBreakOut 로딩씬 검수 후 재설계 — 원본 결함(진행바 미배선·씬이름 문자열 발동·실패 시 영구 대기) 수정판.

## 적용 절차
1. `Core/Loading/`(SceneLoader·LoadingSceneUI) + `Scenes/OutGameLoadingScene.unity` 복사 (.meta째 — GUID 보존)
2. 빌드 세팅 0번에 씬 등록. SceneLoader 인스펙터: `nextSceneName`(로비 씬 이름), `preloadLabel`(비우면 프리로드 생략)
3. 프리로드 대상 에셋에 **Addressables 라벨** 부여 — 주소 접두사는 라벨이 아님 (TongTong InvalidKeyException 실측 교훈).
   라벨 없이 켜도 무해: LoadResourceLocationsAsync 프로브가 조용히 건너뜀
4. 타이틀 텍스트를 게임명으로 교체 (로고 아트 슬롯)

## 확장 지점
- **SDK 초기화 자리**: 광고·분석·IAP 초기화는 이 씬의 SceneLoader.Start 앞단이 정위치
- **전환(씬간) 로딩**: 다음 씬 진입 시 Addressables 대량 로드가 생기면 additive 커버형 로딩(AnimalBreakOut 방식)을 별도 도입 — 그때도 진행바 실배선·실패 폴백은 이 모듈 기준으로
- **Awaitable 전환 방침 (다음 신규 프로젝트)**: 코루틴 대신 Unity 6 async/await(Awaitable) 채택 검토 —
  값 반환·예외 전파·MonoBehaviour 비종속(러너 주입 불필요) 이점. destroyCancellationToken 수명 관리 필수
