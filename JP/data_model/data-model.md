# データモデルとモジュール

ユーザーによって書かれたたくさんのpostを持つblogを考えます.

```mermaid
---
title: a Blog that was written by a User, and has many Posts
---
erDiagram
    USER ||--o| BLOG: owner
    BLOG ||--o{ POST : contains
```

```sql
CREATE TABLE users
(
    id bigserial primary key ,
    name text
);

CREATE TABLE blog
(
    id bigserial primary key ,
    blog_name text,
    owner bigint NOT NULL,
    FOREIGN KEY (owner) REFERENCES users(id)
);

CREATE TABLE post
(
    id bigserial primary key ,
    subject text,
    content text,
    blog_id bigint NOT NULL,
    FOREIGN KEY (blog_id) REFERENCES blog(id)
);
```

## Link, Select で表現するモジュールの関係
- 1 - 1  (1 - 0..1) の関係を表す場合に， 参照するModule（`Blog`）に`Link`(`Select`)フィールドを配置し，参照先のModule(`User`)を設定します.
- Link はモーダルから1レコードを選択します．(複数のフィールドを表示して，選択する場合に使用します．)
- SelectはSelectBoxから1レコード選択します．(名前等，1つのフィールドで選択する場合に使用します．)

<img src="images/1-1_data_model.png" width="400" alt="1対1" title="1対1" style="border: 1px solid;">

画面では参照先のModuleを一覧から選択できるようになります.

<img src="images/1-1_UI.png" width="400" alt="1対1" title="1対1" style="border: 1px solid;">

## List, DetailList, TileList で表現するモジュールの関係
- 1 - N  (1 - 0..N) の関係を表す場合に， 参照するModule（`Blog`）に`List`(`DetailList`, `TileList`)フィールドを配置します.

- 被参照Module
  - 被参照Moduleに(`Blog`)に`List`(`DetailList`, `TileList`)フィールドを配置
  - Conditionに一覧に表示する条件を設定

    <img src="images/1-N_conditions.png" width="400" alt="1対N条件" title="1対N条件" style="border: 1px solid;">
  - 一覧形式でBlogを参照しているPostが表示されます.

    <img src="images/1-N_UI.png" width="400" alt="1対NUI" title="1対NUI" style="border: 1px solid;">


## N - N

多対多の関係

- 従業員は複数の部署を兼務
- 部署には複数の従業員が所属

```mermaid
---
title: employees assigned multiple depatrments
---
erDiagram
    Employee }|--o| Assignment : inChargeOf
  Assignment |o--|{ Department : belong
```