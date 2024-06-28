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

## 1 - 1  (1 - 0..1)
参照するModule（`Blog`）に`Link`フィールドを配置し，参照先のModule(`User`)を設定します.

<img src="images/1-1_data_model.png" width="400" alt="1対1" title="1対1" style="border: 1px solid;">

画面では参照先のModuleを一覧から選択できるようになります.

<img src="images/1-1_UI.png" width="400" alt="1対1" title="1対1" style="border: 1px solid;">

## 1 - N  (1 - 0..N)
- 参照Module 
  - 参照するModule（`Post`）に`Link`フィールドを配置し，参照先のModule(`Blog`)を設定します.(1対1のLinkフィールドと同じ)
 

- 被参照Module
  - 被参照Moduleに(`Blog`)に`List`(`DetailList`, `TileList`)フィールドを配置
  - Conditionに一覧に表示する条件を設定

    <img src="images/1-N_conditions.png" width="400" alt="1対N条件" title="1対N条件" style="border: 1px solid;">
  - 一覧形式でBlogを参照しているPostが表示されます.

    <img src="images/1-N_UI.png" width="400" alt="1対NUI" title="1対NUI" style="border: 1px solid;">


