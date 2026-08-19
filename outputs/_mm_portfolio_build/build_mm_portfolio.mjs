import fs from "node:fs/promises";
import path from "node:path";
import { Presentation, PresentationFile } from "@oai/artifact-tool";

const ROOT = "C:/Users/UserK/Documents/UnityProject/2D_6KProj";
const OUT = path.join(ROOT, "outputs");
const FINAL = path.join(OUT, "MM_2D_Action_BossRush_Portfolio.pptx");
const RENDER = path.join(OUT, "_mm_portfolio_build", "render");
const screenshotPath = path.join(OUT, "_mm_portfolio_build", "gameplay_crop.png");

const W = 1280;
const H = 720;
const C = {
  bg: "#111318",
  bg2: "#171a21",
  ink: "#f4f1e8",
  muted: "#b9b3a5",
  dim: "#7f8792",
  line: "#343946",
  red: "#e45b4f",
  gold: "#f0b94d",
  green: "#61c47a",
  blue: "#6aa6ff",
  codeBg: "#0b0e14",
};

async function writeBlob(file, blob) {
  await fs.writeFile(file, new Uint8Array(await blob.arrayBuffer()));
}

async function maybeImage(file) {
  try {
    const bytes = await fs.readFile(file);
    return bytes.buffer.slice(bytes.byteOffset, bytes.byteOffset + bytes.byteLength);
  } catch {
    return null;
  }
}

function rect(slide, p, fill = C.bg2, line = C.line, radius = "rounded") {
  return slide.shapes.add({
    geometry: "roundRect",
    position: p,
    fill,
    line: { style: "solid", fill: line, width: 1 },
    borderRadius: radius,
  });
}

function text(slide, value, p, style = {}) {
  const shape = slide.shapes.add({
    geometry: "textbox",
    position: p,
    fill: "none",
    line: { style: "solid", fill: "none", width: 0 },
  });
  shape.text = value;
  shape.text.style = {
    fontFace: style.fontFace ?? "Malgun Gothic",
    fontSize: style.fontSize ?? 22,
    color: style.color ?? C.ink,
    bold: style.bold ?? false,
    alignment: style.alignment ?? "left",
    ...style,
  };
  return shape;
}

function title(slide, value, kicker = "MM_2D_6KProj Portfolio") {
  text(slide, kicker, { left: 64, top: 42, width: 420, height: 28 }, {
    fontSize: 15,
    bold: true,
    color: C.gold,
  });
  text(slide, value, { left: 64, top: 82, width: 1080, height: 52 }, {
    fontSize: 36,
    bold: true,
  });
  slide.shapes.add({
    geometry: "rect",
    position: { left: 64, top: 145, width: 1152, height: 2 },
    fill: C.line,
    line: { style: "solid", fill: C.line, width: 0 },
  });
}

function footer(slide, num) {
  text(slide, `조성재 | sjjfire123@naver.com | Unity Version Control | ${String(num).padStart(2, "0")}`, {
    left: 64,
    top: 674,
    width: 760,
    height: 24,
  }, { fontSize: 13, color: C.dim });
}

function bullet(slide, lines, x, y, w, opts = {}) {
  const h = opts.h ?? lines.length * 44 + 8;
  const s = text(slide, lines.map((line) => `- ${line}`).join("\n"), { left: x, top: y, width: w, height: h }, {
    fontSize: opts.fontSize ?? 22,
    color: opts.color ?? C.ink,
  });
  return s;
}

function label(slide, value, x, y, color = C.red) {
  rect(slide, { left: x, top: y, width: 178, height: 34 }, "transparent", color, "rounded");
  text(slide, value, { left: x + 14, top: y + 7, width: 150, height: 22 }, {
    fontSize: 15,
    bold: true,
    color,
    alignment: "center",
  });
}

function card(slide, head, body, x, y, w, h, accent = C.red) {
  rect(slide, { left: x, top: y, width: w, height: h });
  slide.shapes.add({
    geometry: "rect",
    position: { left: x, top: y, width: 8, height: h },
    fill: accent,
    line: { style: "solid", fill: accent, width: 0 },
  });
  text(slide, head, { left: x + 24, top: y + 22, width: w - 48, height: 32 }, {
    fontSize: 23,
    bold: true,
  });
  text(slide, body, { left: x + 24, top: y + 68, width: w - 48, height: h - 88 }, {
    fontSize: 18,
    color: C.muted,
  });
}

function codeSlide(slide, slideTitle, codeTitle, code, insight, num) {
  title(slide, slideTitle);
  text(slide, codeTitle, { left: 72, top: 176, width: 704, height: 34 }, {
    fontSize: 24,
    bold: true,
    color: C.gold,
  });
  rect(slide, { left: 72, top: 218, width: 704, height: 410 }, C.codeBg, C.line, "rounded");
  text(slide, code, { left: 96, top: 242, width: 656, height: 360 }, {
    fontFace: "Consolas",
    fontSize: 14,
    color: "#d8e0ea",
  });
  card(slide, "개발 철학 / 코딩 스타일", insight, 820, 224, 360, 260, C.gold);
  text(slide, "실제 프로젝트 소스 일부를 발췌했습니다.", { left: 820, top: 512, width: 360, height: 72 }, {
    fontSize: 18,
    color: C.dim,
  });
  footer(slide, num);
}

async function main() {
  await fs.mkdir(RENDER, { recursive: true });
  const gameShot = await maybeImage(screenshotPath);
  const p = Presentation.create({ slideSize: { width: W, height: H } });
  let n = 1;

  {
    const s = p.slides.add();
    s.background.fill = C.bg;
    if (gameShot) {
      s.images.add({
        blob: gameShot,
        contentType: "image/png",
        alt: "2D_6KProj gameplay screenshot",
        fit: "cover",
        position: { left: 610, top: 0, width: 670, height: 720 },
      });
      s.shapes.add({
        geometry: "rect",
        position: { left: 540, top: 0, width: 230, height: 720 },
        fill: { color: C.bg, transparency: 18 },
        line: { style: "solid", fill: "none", width: 0 },
      });
    }
    text(s, "MM_2D_6KProj", { left: 72, top: 92, width: 520, height: 70 }, { fontSize: 54, bold: true });
    text(s, "2D Action Boss Rush Portfolio", { left: 72, top: 170, width: 560, height: 42 }, { fontSize: 28, color: C.gold, bold: true });
    text(s, "킹덤 뉴 랜즈형 아이디어에서 메트로베니아를 거쳐,\n취업 포트폴리오 완성을 목표로 3 Stage Boss Rush로 재설계한 Unity 2D 액션 프로젝트입니다.", {
      left: 72, top: 266, width: 560, height: 130,
    }, { fontSize: 22, color: C.muted });
    label(s, "Unity 6000.3.8f1", 72, 438, C.blue);
    label(s, "Pixel Perfect", 270, 438, C.green);
    label(s, "Unity VCS", 468, 438, C.gold);
    text(s, "조성재\nsjjfire123@naver.com", { left: 72, top: 570, width: 480, height: 62 }, { fontSize: 22, color: C.ink, bold: true });
    footer(s, n++);
  }

  {
    const s = p.slides.add(); s.background.fill = C.bg; title(s, "이 프로젝트의 핵심은 스코프를 줄여 완성도를 올린 것입니다");
    card(s, "초기 구상", "킹덤 뉴 랜즈처럼 탐험과 거점 운영이 섞인 횡스크롤 게임을 목표로 시작했습니다.", 72, 190, 330, 250, C.blue);
    card(s, "중간 전환", "메트로베니아 액션 플랫포머로 방향을 바꿨지만, 스토리와 레벨 디자인 범위가 커졌습니다.", 474, 190, 330, 250, C.gold);
    card(s, "최종 결정", "취업 포트폴리오 제출을 위해 3개 스테이지와 보스 전투 중심 구조로 압축했습니다.", 876, 190, 330, 250, C.red);
    text(s, "배운 점: 하고 싶은 게임을 전부 담기보다, 지금의 실력과 일정 안에서 끝까지 완성할 수 있는 구조를 선택했습니다.", {
      left: 120, top: 510, width: 1040, height: 58,
    }, { fontSize: 24, bold: true, color: C.ink, alignment: "center" });
    footer(s, n++);
  }

  {
    const s = p.slides.add(); s.background.fill = C.bg; title(s, "포트폴리오 관점에서 보여주려는 역량");
    bullet(s, [
      "액션 조작: 이동, 점프, 회피, 3타 콤보, 상하단 공격 흐름 구현",
      "전투 피드백: 피격, 넉백, 무적 시간, 히트스톱, 카메라/애니메이션 반응",
      "게임 진행: Stage1 -> Stage2 -> Stage3 -> FinalScene 흐름 관리",
      "저장/이어하기: 위치, 체력, 스태미나, 공격력 저장 및 씬 복원",
      "구조화: EnemyCore와 Brain/Movement/Attack 인터페이스로 적 행동 분리",
    ], 112, 192, 1020, { fontSize: 24, h: 260 });
    card(s, "개발 철학", "큰 시스템을 한 번에 완성하려 하기보다, 플레이어 경험에 직접 닿는 기능부터 작게 완성하고 검증했습니다.", 112, 500, 1020, 96, C.gold);
    footer(s, n++);
  }

  {
    const s = p.slides.add(); s.background.fill = C.bg; title(s, "플레이 루프는 짧고 반복 가능한 보스러쉬 구조입니다");
    card(s, "1. 전투 준비", "메인 메뉴에서 시작하거나 저장 데이터를 불러옵니다.", 90, 210, 260, 210, C.blue);
    card(s, "2. 룸 클리어", "스테이지 안의 전투 룸을 클리어하며 플레이어 능력치를 유지합니다.", 390, 210, 260, 210, C.green);
    card(s, "3. 보스 진입", "조건 달성 후 포탈이 열리고 보스 전투로 이어집니다.", 690, 210, 260, 210, C.gold);
    card(s, "4. 다음 스테이지", "PlayerRuntimeData가 HP/ST/DMG를 다음 씬까지 전달합니다.", 990, 210, 220, 210, C.red);
    text(s, "설계 의도: 스토리/거대 맵 제작 부담을 줄이고, 액션 전투와 시스템 완성도에 시간을 집중했습니다.", {
      left: 96, top: 510, width: 1080, height: 58,
    }, { fontSize: 24, color: C.ink, bold: true, alignment: "center" });
    footer(s, n++);
  }

  {
    const s = p.slides.add(); s.background.fill = C.bg; title(s, "플레이어 액션은 입력 반응성과 캔슬 감각을 우선했습니다");
    bullet(s, [
      "Z: 점프, X: 공격, C: 회피처럼 키 입력을 단순화해 테스트 효율을 높였습니다.",
      "공격 중 공중 이동 배율과 회피 캔슬 여부를 분리해 조작감을 조정할 수 있게 했습니다.",
      "체력과 스태미나는 UI와 저장 데이터 양쪽에 연결해 전투 결과가 다음 흐름에 남도록 했습니다.",
    ], 96, 190, 680, { fontSize: 24, h: 210 });
    card(s, "조작 설계", "플레이어가 이해해야 하는 규칙은 줄이고, 실제 전투에서 느끼는 선택지는 남기는 방향으로 구성했습니다.", 840, 190, 340, 176, C.green);
    card(s, "튜닝 포인트", "공격 쿨타임, 콤보 리셋, 회피 지속/쿨타임, 스태미나 회복량은 인스펙터에서 빠르게 조정할 수 있습니다.", 840, 406, 340, 176, C.blue);
    footer(s, n++);
  }

  {
    const s = p.slides.add(); s.background.fill = C.bg; title(s, "보스 3종은 공통 구조 위에 다른 전투 리듬을 얹었습니다");
    card(s, "Stage 1: Golem", "패턴 인지와 기본 회피를 확인하는 첫 보스. 공격 전조와 거리 감각을 학습시키는 역할입니다.", 84, 190, 330, 280, C.gold);
    card(s, "Stage 2: Blood King", "압박감과 패턴 변화가 중심인 중반 보스. 피격 리스크와 공격 타이밍 판단을 요구합니다.", 474, 190, 330, 280, C.red);
    card(s, "Stage 3: Heart Hoarder", "최종 스테이지 보스. 앞선 전투에서 익힌 회피/공격/자원 관리를 종합합니다.", 864, 190, 330, 280, C.green);
    text(s, "공통 요소: BossBattleManager, BossIntroUI, 클리어 후 포탈/씬 전환 흐름", {
      left: 160, top: 535, width: 960, height: 40,
    }, { fontSize: 24, color: C.muted, alignment: "center" });
    footer(s, n++);
  }

  {
    const s = p.slides.add(); s.background.fill = C.bg; title(s, "저장 시스템은 위치 저장에서 상태 저장까지 확장했습니다");
    bullet(s, [
      "초기 저장: 현재 씬 이름과 플레이어 위치만 PlayerPrefs에 저장",
      "확장 저장: maxHP, currentHP, maxST, damage까지 함께 저장",
      "문제 해결: Main Menu에서 플레이어 참조가 없던 문제를 Save 시점 참조 갱신으로 해결",
      "주의한 점: New Game과 Continue의 데이터 적용 순서를 분리해 새 게임 오염을 막았습니다.",
    ], 96, 190, 1040, { fontSize: 24, h: 230 });
    card(s, "코딩 스타일", "null 가능성이 있는 참조는 저장 직전에 다시 찾고, 불러오기 함수는 기본값을 반환하게 만들어 실패 시 게임 흐름을 유지했습니다.", 96, 490, 1040, 96, C.gold);
    footer(s, n++);
  }

  {
    const s = p.slides.add(); s.background.fill = C.bg;
    codeSlide(s, "소스코드 1: SaveManager는 저장 시점에 참조를 다시 확인합니다", "Assets/_Project/01_Scripts/Common/SaveManager.cs",
`private void RefreshReferences()
{
    player = FindAnyObjectByType<Player>();
    playerHealth = FindAnyObjectByType<PlayerHealth>();
}

public void Save(Vector3 playerPosition)
{
    RefreshReferences();
    if (player == null || playerHealth == null)
    {
        Debug.LogWarning("Save failed: Player or PlayerHealth not found.");
        return;
    }

    PlayerPrefs.SetString(SceneKey, SceneManager.GetActiveScene().name);
    PlayerPrefs.SetFloat(SaveXKey, playerPosition.x);
    PlayerPrefs.SetInt(SaveMaxHPKey, playerHealth.maxHP);
    PlayerPrefs.SetInt(SaveCurrentHPKey, playerHealth.currentHP);
    PlayerPrefs.SetInt(SaveMaxSTKey, player.maxStamina);
    PlayerPrefs.SetFloat(SaveDMGKey, player.attackDamage);
    PlayerPrefs.Save();
}`, "메뉴 씬처럼 Player가 없는 상태에서 Awake 참조를 고정하면 저장이 실패할 수 있었습니다. 그래서 Save 호출 순간에 실제 씬 오브젝트를 다시 찾고, 실패 시 경고 후 return하는 방어적 흐름으로 정리했습니다.", n++);
  }

  {
    const s = p.slides.add(); s.background.fill = C.bg;
    codeSlide(s, "소스코드 2: 씬 전환 중 유지할 데이터는 RuntimeData로 분리했습니다", "Assets/_Project/01_Scripts/Common/PlayerRuntimeData.cs",
`public class PlayerRuntimeData : MonoBehaviour
{
    public static PlayerRuntimeData Instance { get; private set; }
    public bool HasData { get; private set; }

    public int maxHP, currentHP;
    public int maxStamina, currentStamina;
    public float attackDamage;

    public void SaveFromPlayer(Player player, PlayerHealth health)
    {
        if (player == null || health == null) return;

        maxHP = health.MaxHP;
        currentHP = health.CurrentHP;
        maxStamina = player.MaxStaminaInt;
        currentStamina = player.CurrentStaminaInt;
        attackDamage = player.attackDamage;
        HasData = true;
    }
}`, "영구 저장과 씬 전환용 임시 상태를 섞지 않도록 역할을 나눴습니다. 저장 파일은 Continue용, RuntimeData는 Stage1->2->3 진행 중 능력치 유지용으로 사용했습니다.", n++);
  }

  {
    const s = p.slides.add(); s.background.fill = C.bg;
    codeSlide(s, "소스코드 3: 적 AI는 판단, 이동, 공격 책임을 나눴습니다", "Assets/_Project/01_Scripts/Enemies/EnemyCore.cs",
`public interface IEnemyBrain
{
    void Tick(EnemyCore core, float dt);
}

public class EnemyCore : MonoBehaviour
{
    private void Awake()
    {
        Brain = GetComponent<IEnemyBrain>();
        Movement = GetComponent<IEnemyMovement>();
        Attack = GetComponent<IEnemyAttack>();
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        Brain.Tick(this, dt);
        Movement?.Tick(this, dt);
        Attack?.Tick(this, dt);
    }
}`, "EnemyCore가 모든 행동을 직접 처리하지 않고, Brain/Movement/Attack 컴포넌트를 조합합니다. 새 적을 만들 때 기존 코어를 유지한 채 행동 모듈만 바꿀 수 있는 구조를 의도했습니다.", n++);
  }

  {
    const s = p.slides.add(); s.background.fill = C.bg; title(s, "문제 해결 사례는 기능보다 더 강한 포트폴리오 근거입니다");
    card(s, "저장 데이터 적용 순서", "위치 로드가 ContinueMode를 먼저 끄면 HP/ST/DMG가 적용되지 않는 문제가 있었습니다. 런타임 데이터 -> 저장 상태 -> 위치 순서로 정리했습니다.", 82, 190, 350, 300, C.gold);
    card(s, "Stage 메뉴 패널 미표시", "Stage1/2/3 Canvas scale이 0으로 저장되어 MenuPanel이 활성화돼도 보이지 않았습니다. 씬과 StagePrefab의 Canvas scale을 1로 수정했습니다.", 466, 190, 350, 300, C.red);
    card(s, "보스러쉬 전환", "완성 가능성을 위해 거대 맵과 스토리 비중을 줄이고, 전투/저장/진행 시스템 검증에 집중했습니다.", 850, 190, 350, 300, C.green);
    text(s, "면접에서 강조할 문장: 문제를 발견하면 원인을 씬 데이터, 실행 순서, 참조 생명주기 중 어디에 있는지 나눠서 추적했습니다.", {
      left: 120, top: 540, width: 1040, height: 52,
    }, { fontSize: 22, bold: true, color: C.muted, alignment: "center" });
    footer(s, n++);
  }

  {
    const s = p.slides.add(); s.background.fill = C.bg; title(s, "UI/UX는 플레이 중 흐름이 끊기지 않는 것을 목표로 했습니다");
    bullet(s, [
      "Pause 메뉴: Escape 입력으로 전투 중 일시정지와 복귀를 처리",
      "Game Over: CanvasGroup 기반으로 사망 후 상태 전환을 관리",
      "Boss Intro: 보스전 시작 전 짧은 연출로 전투 진입 신호를 제공",
      "Pixel Perfect Camera: 320x180 기준, 1920x1080에서 6배 정수 스케일 권장",
    ], 104, 190, 990, { fontSize: 24, h: 236 });
    card(s, "포트폴리오 포인트", "화려한 기능보다 플레이어가 길을 잃지 않고 전투에 집중하도록 만드는 기본 UX를 우선했습니다.", 104, 500, 990, 88, C.blue);
    footer(s, n++);
  }

  {
    const s = p.slides.add(); s.background.fill = C.bg; title(s, "개발 환경과 협업 이력은 GitHub 대신 Unity Version Control로 정리합니다");
    card(s, "Engine", "Unity 6000.3.8f1 / URP / 2D Pixel Perfect Camera", 104, 190, 500, 120, C.blue);
    card(s, "Version Control", "Unity Version Control 사용\nGitHub 링크 항목은 비워두거나 삭제해도 됩니다.", 676, 190, 500, 120, C.gold);
    card(s, "Build Target", "Windows 실행 빌드 우선, itch.io 업로드 시 WebGL 빌드도 함께 제공하면 접근성이 좋아집니다.", 104, 370, 500, 140, C.green);
    card(s, "Portfolio Files", "제출 파일명과 링크 설명에는 반드시 MM_ 접두사를 붙입니다.", 676, 370, 500, 140, C.red);
    footer(s, n++);
  }

  {
    const s = p.slides.add(); s.background.fill = C.bg; title(s, "제출 링크 설명은 MM_ 접두사를 붙여 관리합니다");
    rect(s, { left: 110, top: 198, width: 1060, height: 330 }, C.codeBg, C.line, "rounded");
    text(s,
`MM_포트폴리오_PPT
  - 파일: MM_2D_Action_BossRush_Portfolio.pptx

MM_실행빌드링크
  - 예: itch.io 또는 Google Drive에 업로드한 Windows ZIP 링크

MM_플레이영상링크
  - 예: YouTube 일부 공개 플레이 영상 링크

MM_UnityVersionControl
  - GitHub 대신 Unity Version Control을 사용했다고 명시`,
      { left: 150, top: 230, width: 980, height: 270 },
      { fontFace: "Consolas", fontSize: 23, color: C.ink });
    text(s, "나중에 URL만 채우면 제출 양식에서도 규칙이 흐트러지지 않습니다.", {
      left: 146, top: 560, width: 980, height: 40,
    }, { fontSize: 22, color: C.gold, bold: true, alignment: "center" });
    footer(s, n++);
  }

  {
    const s = p.slides.add(); s.background.fill = C.bg; title(s, "마지막으로 보여줄 메시지");
    text(s, "완성 가능한 범위로 스코프를 줄이고,\n액션 조작-전투 피드백-저장/진행 시스템을 끝까지 연결한 Unity 2D 보스러쉬 프로젝트입니다.", {
      left: 110, top: 210, width: 1060, height: 150,
    }, { fontSize: 34, bold: true, alignment: "center" });
    bullet(s, [
      "본인이 작성한 핵심 소스코드 포함",
      "개발 철학: 작은 완성, 명확한 책임 분리, 플레이 흐름 우선",
      "코딩 스타일: 방어적 null 처리, 싱글턴 생명주기 관리, 컴포넌트 조합 구조",
    ], 180, 430, 900, { fontSize: 24, h: 150 });
    text(s, "조성재 | sjjfire123@naver.com", { left: 340, top: 612, width: 600, height: 34 }, {
      fontSize: 26,
      color: C.gold,
      bold: true,
      alignment: "center",
    });
    footer(s, n++);
  }

  for (const [i, slide] of p.slides.items.entries()) {
    const png = await p.export({ slide, format: "png", scale: 1 });
    await writeBlob(path.join(RENDER, `slide-${String(i + 1).padStart(2, "0")}.png`), png);
    const layout = await slide.export({ format: "layout" });
    await fs.writeFile(path.join(RENDER, `slide-${String(i + 1).padStart(2, "0")}.layout.json`), await layout.text());
  }
  const montage = await p.export({ format: "webp", montage: true, scale: 1 });
  await writeBlob(path.join(RENDER, "montage.webp"), montage);

  const pptx = await PresentationFile.exportPptx(p);
  await pptx.save(FINAL);
  console.log(FINAL);
}

main().catch((err) => {
  console.error(err);
  process.exitCode = 1;
});
