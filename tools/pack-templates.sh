#!/bin/zsh
# Hub 템플릿 2개(가로형·세로형)를 한 리포에서 패킹한다(2026-10-29). develop 머지 뒤에 돌린다.
# 리포에 커밋된 기본은 가로형이다. 세로형은 OrientationPreset.ApplyPortrait를 배치로 적용해 패킹한 뒤 git으로 되돌린다.
set -e
REPO=$(cd "$(dirname "$0")/.." && pwd)
OUT="$HOME/Unity user templates"
cd "$REPO"
if [ -n "$(git status --porcelain)" ]; then echo "작업 트리가 깨끗해야 한다(세로형 적용 뒤 git으로 되돌리므로)"; exit 1; fi

pack() {
  rm -rf Library Logs obj *.slnx .vscode
  unity templates pack . --output "$OUT/com.pmj.template.mobilegame-$1-1.0.0.tgz" \
    --name "com.pmj.template.mobilegame-$1" --display-name "PMJ Mobile Game Template ($2)" \
    --template-version 1.0.0 --keep-project-settings --overwrite
}

pack landscape "가로형"
unity run . --timeout 900 --no-tail -- -executeMethod OrientationPreset.ApplyPortrait
pack portrait "세로형"
git checkout -- Assets ProjectSettings
git clean -fdq Assets
echo "완료: $OUT/com.pmj.template.mobilegame-{landscape,portrait}-1.0.0.tgz"
