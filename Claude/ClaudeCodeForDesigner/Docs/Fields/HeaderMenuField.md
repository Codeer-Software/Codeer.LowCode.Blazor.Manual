# HeaderMenuField / SidebarMenuField - ナビゲーションメニュー

## HeaderMenuFieldDesign

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.HeaderMenuFieldDesign`

ページフレームのヘッダー部分に表示されるナビゲーションメニューコンポーネント。`FieldDesignBase` を直接継承する。

## SidebarMenuFieldDesign

**TypeFullName:** `Codeer.LowCode.Blazor.Repository.Design.SidebarMenuFieldDesign`

ページフレームのサイドバー部分に表示されるナビゲーションメニューコンポーネント。`FieldDesignBase` を直接継承する。

## C# クラス定義 (真実の源)

```csharp
public class HeaderMenuFieldDesign : FieldDesignBase
{
    public HeaderMenuType Type { get; set; } = HeaderMenuType.Items;   // enum: Items / カスタム種別
    // 親 FieldDesignBase から継承: Name, IgnoreModification, OnValidateInput
}

public class SidebarMenuFieldDesign : FieldDesignBase
{
    public SidebarPlacement Placement { get; set; } = SidebarPlacement.Left;   // enum: Left / Right
    public SidebarMenuType Type { get; set; } = SidebarMenuType.Items;
    // 親 FieldDesignBase から継承: Name, IgnoreModification, OnValidateInput
}
```

## プロパティ

> 共通プロパティ（Name, IgnoreModification）は [_FieldCommon.md](_FieldCommon.md) を参照。

### HeaderMenuField のプロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Type` | HeaderMenuType | `"Items"` | メニュー種別。 |

### SidebarMenuField のプロパティ

| プロパティ | 型 | デフォルト | 説明 |
|---|---|---|---|
| `Placement` | SidebarPlacement | `"Left"` | サイドバーの配置位置。`Left` / `Right`。 |
| `Type` | SidebarMenuType | `"Items"` | メニュー種別。 |

## 列挙型

### HeaderMenuType

| 値 | 説明 |
|---|---|
| `Home` | ホームリンクを表示する |
| `Items` | ページフレームで定義されたナビゲーションリンク一覧を表示する |
| `UserName` | ログインユーザー名を表示する |
| `Logout` | ログアウトリンクを表示する |

### SidebarMenuType

| 値 | 説明 |
|---|---|
| `Home` | ホームリンクを表示する |
| `Items` | ページフレームで定義されたナビゲーションリンク一覧を表示する |
| `UserName` | ログインユーザー名を表示する |
| `Logout` | ログアウトリンクを表示する |

### SidebarPlacement

| 値 | 説明 |
|---|---|
| `Left` | 左サイドバー |
| `Right` | 右サイドバー |

## JSON例

### HeaderMenuField - ナビゲーションリンク

```json
{
  "Type": "Items",
  "IgnoreModification": false,
  "Name": "HeaderMenu",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.HeaderMenuFieldDesign"
}
```

### HeaderMenuField - ホームリンク

```json
{
  "Type": "Home",
  "IgnoreModification": false,
  "Name": "HeaderHome",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.HeaderMenuFieldDesign"
}
```

### HeaderMenuField - ユーザー名表示

```json
{
  "Type": "UserName",
  "IgnoreModification": false,
  "Name": "HeaderUserName",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.HeaderMenuFieldDesign"
}
```

### HeaderMenuField - ログアウト

```json
{
  "Type": "Logout",
  "IgnoreModification": false,
  "Name": "HeaderLogout",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.HeaderMenuFieldDesign"
}
```

### SidebarMenuField - 左サイドバーにナビゲーション

```json
{
  "Placement": "Left",
  "Type": "Items",
  "IgnoreModification": false,
  "Name": "SidebarMenu",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SidebarMenuFieldDesign"
}
```

### SidebarMenuField - 右サイドバーにユーザー名

```json
{
  "Placement": "Right",
  "Type": "UserName",
  "IgnoreModification": false,
  "Name": "SidebarUserName",
  "TypeFullName": "Codeer.LowCode.Blazor.Repository.Design.SidebarMenuFieldDesign"
}
```

## ランタイム動作

- ページフレーム（`.frm.json`）の定義に基づいてナビゲーションメニューをレンダリングする。
- 通常はページフレームのレイアウト内で使用し、個別のモジュール定義では使用しない。
- **HeaderMenuField:** ヘッダーバー内にメニューを配置する。`Type` により表示する内容が切り替わる。
- **SidebarMenuField:** サイドバー内にメニューを配置する。`Placement` で左右を指定し、`Type` により内容が切り替わる。
- `Items` タイプではページフレームの `Links` 配列で定義されたナビゲーションリンクが表示される。
- DB列マッピングなし。検索対象外。

---

## DOM構造（CSS用）

### HeaderMenuField

#### Home（ブランド表示）

```html
<a class="navbar-brand" href="[URL]" data-system="topbarBrand">
  <span class="[Iconクラス] me-2" aria-hidden="true"></span>
  ブランドテキスト
</a>
```

#### Items（ナビゲーション）

```html
<ul class="navbar-nav me-auto">
  <!-- 子なしメニュー -->
  <li class="nav-item">
    <a class="nav-link" href="[URL]">
      <span class="[Iconクラス] me-2" aria-hidden="true"></span>
      メニュータイトル
    </a>
  </li>

  <!-- 子ありメニュー（ドロップダウン） -->
  <li class="nav-item dropdown">
    <a class="nav-link dropdown-toggle" href="#">タイトル</a>
    <div class="dropdown-menu show">
      <a class="dropdown-item" href="[URL]">子メニュー1</a>
      <a class="dropdown-item" href="[URL]">子メニュー2</a>
    </div>
  </li>
</ul>
```

#### UserName / Logout

```html
<ul class="navbar-nav ms-auto">
  <li class="nav-item">
    <span class="nav-link">ユーザー名</span>
  </li>
  <a class="nav-link" data-system="logout" style="cursor:pointer">
    <span class="oi oi-account-logout" aria-hidden="true"></span>
    Logout
  </a>
</ul>
```

### SidebarMenuField

#### Home（ブランド表示）

```html
<div class="top-row ps-3 navbar" data-system="sidebar-brand">
  <div class="container-fluid">
    <a class="navbar-brand" href="[URL]">
      <span class="[Iconクラス] me-2" aria-hidden="true"></span>
      ブランドテキスト
    </a>
  </div>
</div>
```

#### Items（ナビゲーション）

```html
<nav>
  <!-- メニュー項目 -->
  <div class="nav-item px-3">
    <a class="nav-link" href="[URL]">
      <span class="[Iconクラス] me-2" aria-hidden="true"></span>
      メニュータイトル
    </a>
  </div>

  <!-- 子ありメニュー（展開可能） -->
  <div class="nav-item px-3">
    <a class="nav-link d-flex justify-content-between" href="#">
      <span>タイトル</span>
      <i class="bi bi-chevron-down"></i>
    </a>
    <nav class="nav-children flex-column" style="--sidebar-item-depth: 1">
      <div class="nav-item px-3">
        <a class="nav-link" href="[URL]">子メニュー</a>
      </div>
    </nav>
  </div>
</nav>
```

#### UserName / Logout

```html
<nav>
  <div class="nav-item px-3">
    <span class="nav-link">
      <i class="bi bi-person me-2" aria-hidden="true"></i>
      ユーザー名
    </span>
  </div>
  <div class="nav-item px-3">
    <a class="nav-link" data-system="logout">
      <span class="oi oi-account-logout me-2" aria-hidden="true"></span>
      Logout
    </a>
  </div>
</nav>
```

### CSSセレクタ例

```css
/* ヘッダーのブランド */
.navbar-brand {
  font-weight: bold;
  font-size: 1.2rem;
}

/* サイドバーのナビリンク */
.nav-link {
  padding: 0.5rem 1rem;
  color: rgba(255, 255, 255, 0.8);
}

.nav-link.active {
  color: #fff;
  background-color: rgba(255, 255, 255, 0.1);
}

/* ヘッダーのドロップダウン */
.navbar-nav .dropdown-menu {
  border-radius: 0.25rem;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

/* サイドバーの子メニュー */
.nav-children .nav-link {
  padding-left: calc(1rem + var(--sidebar-item-depth, 0) * 1rem);
  font-size: 0.9rem;
}
```
