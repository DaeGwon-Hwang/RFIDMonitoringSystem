-- 289, 290라인의 dbf 파일 위치 수정하여 쿼리 실행
-- tibero의 경우  289, 290라인의 LOGGING 옵션을 제외하시고 실행


------------------------------------------------------------------------------
-- 기존 테이블 백업 
------------------------------------------------------------------------------
CREATE TABLE TB_ZZREQEXCELLIST_TEMP AS SELECT * FROM TB_ZZREQEXCELLIST;

CREATE TABLE TB_STMENU_TEMP AS SELECT * FROM TB_STMENU;

CREATE TABLE TB_STMENULINK_TEMP AS SELECT * FROM TB_STMENULINK;

CREATE TABLE TB_STMENUGRANTLINK_TEMP AS SELECT * FROM TB_STMENUGRANTLINK;

CREATE TABLE TB_STFOLDERQUERY_TEMP AS SELECT * FROM TB_STFOLDERQUERY;

CREATE TABLE TB_STRECORDQUERY_TEMP AS SELECT * FROM TB_STRECORDQUERY;



------------------------------------------------------------------------------
-- 엑셀 저장 공통 변경에 따른 TABLE 변경
------------------------------------------------------------------------------

-- 생성된 엑셀 파일의 전체 경로를 저장 하는 컬럼 삭제
ALTER TABLE TB_ZZREQEXCELLIST DROP(FULL_PATH);

-- 생성된 엑셀 파일의 KEY 를 저장 하는 컬럼 추가
ALTER TABLE TB_ZZREQEXCELLIST ADD(FILE_KEY VARCHAR2(100));

COMMENT ON COLUMN TB_ZZREQEXCELLIST.FILE_KEY IS '파일 KEY';

-- 생성된 엑셀 파일의 INDEX 를 저장 하는 컬럼 추가
ALTER TABLE TB_ZZREQEXCELLIST ADD(REQ_INDEX VARCHAR2(1));

COMMENT ON COLUMN TB_ZZREQEXCELLIST.REQ_INDEX IS 'INDEX';

-- 자체 감리 조치에 의하여 자료사전에 맞게 컬럼 타입 변경
ALTER TABLE TB_ZZREQEXCELLIST MODIFY(FILE_NM VARCHAR2(500));


------------------------------------------------------------------------------
-- 공통 파일 관리 TABLE 추가
------------------------------------------------------------------------------

CREATE TABLE TB_STFILEMNG
(
  FILE_KEY   VARCHAR2(100 BYTE)                 NOT NULL,
  FILE_NM    VARCHAR2(500 BYTE)                 NOT NULL,
  FILE_SIZE  NUMBER(10)                         NOT NULL,
  FILE_PATH  VARCHAR2(500 BYTE)                 NOT NULL
)
TABLESPACE TS_STDAT01
PCTFREE 5
PCTUSED 60
LOGGING
NOCACHE
NOPARALLEL
;

COMMENT ON TABLE TB_STFILEMNG IS '파일관리';

COMMENT ON COLUMN TB_STFILEMNG.FILE_KEY IS '파일KEY';

COMMENT ON COLUMN TB_STFILEMNG.FILE_NM IS '파일명';

COMMENT ON COLUMN TB_STFILEMNG.FILE_SIZE IS '파일용량';

COMMENT ON COLUMN TB_STFILEMNG.FILE_PATH IS '파일경로(암호화)';

CREATE UNIQUE INDEX TB_STFILEMNG_PK ON TB_STFILEMNG (FILE_KEY)
TABLESPACE TS_STIDX01
NOPARALLEL
;

ALTER TABLE TB_STFILEMNG ADD (
  CONSTRAINT TB_STFILEMNG_PK
  PRIMARY KEY
  (FILE_KEY)
  USING INDEX TB_STFILEMNG_PK
  ENABLE VALIDATE);


--필요한 경우 시노님 생성 및 권한부여 부분 주석 해제 후 실행


CREATE OR REPLACE SYNONYM RMSUSR01.TB_STFILEMNG FOR TB_STFILEMNG;

GRANT DELETE, INSERT, SELECT, UPDATE ON TB_STFILEMNG TO RMSUSR01;


------------------------------------------------------------------------------
-- ID GEN egov 공통컴포넌트 용 TABLE 추가
------------------------------------------------------------------------------

CREATE TABLE COMTECOPSEQ
(
    TABLE_NAME            VARCHAR2(20)  NOT NULL ,
    NEXT_ID               NUMBER(30)  NULL
)
TABLESPACE TS_STDAT01
LOGGING 
NOCACHE
NOPARALLEL
;


COMMENT ON COLUMN COMTECOPSEQ.TABLE_NAME IS '테이블명';

COMMENT ON COLUMN COMTECOPSEQ.NEXT_ID IS '다음ID';


ALTER TABLE COMTECOPSEQ ADD
(
    CONSTRAINT COMTECOPSEQ_PK PRIMARY KEY (TABLE_NAME)
    USING
        INDEX TABLESPACE TS_STIDX01
        PCTFREE 5
)
;

INSERT INTO COMTECOPSEQ ( TABLE_NAME, NEXT_ID ) VALUES ('RMS_FILE_ID', 1);

--필요한 경우 시노님 생성 및 권한부여 부분 주석 해제 후 실행


CREATE OR REPLACE SYNONYM RMSUSR01.COMTECOPSEQ FOR COMTECOPSEQ;

GRANT DELETE, INSERT, SELECT, UPDATE ON COMTECOPSEQ TO RMSUSR01;


------------------------------------------------------------------------------
-- 장기보존포맷 인증서 장기검증 이력 TABLE 추가
------------------------------------------------------------------------------

CREATE TABLE TB_STFORMATVERIFYHIST
(
  RECORD_CENTER_ID  VARCHAR2(7 BYTE)        NOT NULL,
  NEO_GUBUN         VARCHAR2(1 BYTE)        NOT NULL,
  TRGT_ID           VARCHAR2(14 BYTE)       NOT NULL,
  REQ_USER_ID       VARCHAR2(10 BYTE)       NOT NULL,
  REQ_DTIME         VARCHAR2(14 BYTE)       NOT NULL,
  RESULT_DIV_CD     VARCHAR2(1 BYTE)        NOT NULL,
  RESULT_CODE       VARCHAR2(5 BYTE),
  RESULT_MSG        VARCHAR2(500 BYTE)
)
TABLESPACE TS_STDAT01
PCTUSED    0
PCTFREE    10
INITRANS   1
MAXTRANS   255
STORAGE    (
            INITIAL          64K
            NEXT             1M
            MINEXTENTS       1
            MAXEXTENTS       UNLIMITED
            PCTINCREASE      0
            BUFFER_POOL      DEFAULT
           )
NOLOGGING 
NOCOMPRESS 
NOCACHE
NOPARALLEL
MONITORING;

COMMENT ON TABLE TB_STFORMATVERIFYHIST IS '장기보존포맷 인증서 장기검증 이력';

COMMENT ON COLUMN TB_STFORMATVERIFYHIST.RECORD_CENTER_ID IS '기록관ID';

COMMENT ON COLUMN TB_STFORMATVERIFYHIST.NEO_GUBUN IS 'NEO구분 (1:철,2:건)';

COMMENT ON COLUMN TB_STFORMATVERIFYHIST.TRGT_ID IS '대상기록물ID (기록물철ID/기록물건ID)';

COMMENT ON COLUMN TB_STFORMATVERIFYHIST.REQ_USER_ID IS '요청자ID';

COMMENT ON COLUMN TB_STFORMATVERIFYHIST.REQ_DTIME IS '요청일시';

COMMENT ON COLUMN TB_STFORMATVERIFYHIST.RESULT_DIV_CD IS '결과구분 (1:성공,2:실패)';

COMMENT ON COLUMN TB_STFORMATVERIFYHIST.RESULT_CODE IS '결과코드';

COMMENT ON COLUMN TB_STFORMATVERIFYHIST.RESULT_MSG IS '결과메시지';


--시노님 생성 및 권한부여 부분 주석 해제 후 실행
CREATE OR REPLACE SYNONYM RMSUSR01.TB_STFORMATVERIFYHIST FOR TB_STFORMATVERIFYHIST;

GRANT DELETE, INSERT, SELECT, UPDATE ON TB_STFORMATVERIFYHIST TO RMSUSR01;



------------------------------------------------------------------------------
-- 장기보존포멧 검증현황 메뉴 추가 스크립트
------------------------------------------------------------------------------

--TB_STMENULINK 
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM, LINK_IMG_LEFT, LINK_CSS)
 Values (447, '2', '검증현황', 'cm/left/left_title_ACCS03.gif', 'ACCS.css');
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM)
 Values (448, '3', '장기보존포멧검증현황');
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM, LINK_URL)
 Values (449, '4', '년도별검증현황', 'navigateVerifyStatusByYearList.rms');
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM, LINK_URL)
 Values (450, '4', '기록물별검증현황', 'navigateVerifyStatusByDocList.rms');

--TB_STMENU 
Insert into TB_STMENU (RECORD_CENTER_ID, MENU_ID, MENU_NM, UPPER_MENU_ID, MENU_DIV_CD, MENU_SORT_SNO, USE_FLAG, MENU_LINK_SNO, WORK_DTIME)
(
    select record_center_id, '447', '검증현황', '6', '2', 3, '1', 447, TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
    from tb_strecordcenter  A
    where not exists (
        select 1
        from tb_stmenu b
        where A.record_center_id = B.record_center_id
        and B.menu_id = '447'
    )
);


Insert into TB_STMENU (RECORD_CENTER_ID, MENU_ID, MENU_NM, UPPER_MENU_ID, MENU_DIV_CD, MENU_SORT_SNO, USE_FLAG, MENU_LINK_SNO, WORK_DTIME)
(
    select record_center_id, '448', '장기보존포멧검증현황', '447', '3', 1, '1', 448, TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
    from tb_strecordcenter  A
    where not exists (
        select 1
        from tb_stmenu b
        where A.record_center_id = B.record_center_id
        and B.menu_id = '448'
    )
);

Insert into TB_STMENU (RECORD_CENTER_ID, MENU_ID, MENU_NM, UPPER_MENU_ID, MENU_DIV_CD, MENU_SORT_SNO, USE_FLAG, MENU_LINK_SNO, MENU_GRANT_LEVL, WORK_DTIME)
(
    select record_center_id, '450', '기록물별검증현황', '448', '4', 2, '1', 450, '3', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
    from tb_strecordcenter  A
    where not exists (
        select 1
        from tb_stmenu b
        where A.record_center_id = B.record_center_id
        and B.menu_id = '450'
    )
);

Insert into TB_STMENU (RECORD_CENTER_ID, MENU_ID, MENU_NM, UPPER_MENU_ID, MENU_DIV_CD, MENU_SORT_SNO, USE_FLAG, MENU_LINK_SNO, MENU_GRANT_LEVL, WORK_DTIME)
(
    select record_center_id, '449', '년도별검증현황', '448', '4', 1, '1', 449, '3', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
    from tb_strecordcenter  A
    where not exists (
        select 1
        from tb_stmenu b
        where A.record_center_id = B.record_center_id
        and B.menu_id = '449'
    )
);

--TB_STMENUGRANTLINK 
Insert into TB_STMENUGRANTLINK (RECORD_CENTER_ID, MENU_GRANT_GRP_CD, MENU_ID, WORK_DTIME)
(
    select record_center_id, '003', '449', TO_CHAR(SYSDATE, 'YYYYMMDDHH24MISS') 
    from tb_strecordcenter A
    where not exists (
        select 1 
        from tb_stmenugrantlink b
        where a.record_center_id = b.record_center_id
        and menu_grant_grp_cd = '003'
        and menu_id = '449'
    )
);
 
 
Insert into TB_STMENUGRANTLINK (RECORD_CENTER_ID, MENU_GRANT_GRP_CD, MENU_ID, WORK_DTIME)
(
    select record_center_id, '003', '450', TO_CHAR(SYSDATE, 'YYYYMMDDHH24MISS') 
    from tb_strecordcenter A
    where not exists (
        select 1 
        from tb_stmenugrantlink b
        where a.record_center_id = b.record_center_id
        and menu_grant_grp_cd = '003'
        and menu_id = '450'
    )
);

------------------------------------------------------------------------------
-- 통합정보공개시스템 연계용 DB 생성
------------------------------------------------------------------------------

CREATE TABLESPACE TS_OPDAT01 LOGGING DATAFILE '/oradata/RMSORA/DAT/TS_OPDAT01_01.dbf' SIZE 1000M  EXTENT MANAGEMENT LOCAL SEGMENT SPACE MANAGEMENT AUTO;
CREATE TABLESPACE TS_OPIDX01 LOGGING DATAFILE '/oradata/RMSORA/IDX/TS_OPIDX01_01.dbf' SIZE 500M   EXTENT MANAGEMENT LOCAL SEGMENT SPACE MANAGEMENT AUTO;

CREATE USER RMSOPEN01 IDENTIFIED BY RMSOPEN01 DEFAULT TABLESPACE TS_OPDAT01 TEMPORARY TABLESPACE TEMP;
GRANT CONNECT, RESOURCE TO RMSOPEN01;

-- TB_OPOPENGROUP

CREATE TABLE TB_OPOPENGROUP
(
  RECORD_CENTER_ID   VARCHAR2(7 BYTE)               NOT NULL,
  OPEN_SYS_CD        VARCHAR2(2 BYTE)               NOT NULL,
  OPEN_DTIME         VARCHAR2(14 BYTE)              NOT NULL,
  CHOICE_CNT         NUMBER(10)         DEFAULT 0   NOT NULL,
  EXCEPT_CNT         NUMBER(10)         DEFAULT 0   NOT NULL,
  APPEND_CNT         NUMBER(10)         DEFAULT 0   NOT NULL,
  LIST_TYPE_CD       VARCHAR2(2 BYTE)               NOT NULL,
  LIST_PROV_FLAG     VARCHAR2(1 BYTE)   DEFAULT '0' NOT NULL,
  LIST_PROV_DTIME    VARCHAR2(14 BYTE)              NULL,
  REG_DTIME          VARCHAR2(14 BYTE)              NOT NULL
)
TABLESPACE TS_OPDAT01
LOGGING
NOCACHE
NOPARALLEL;

COMMENT ON TABLE TB_OPOPENGROUP IS '공개계획';
COMMENT ON COLUMN TB_OPOPENGROUP.RECORD_CENTER_ID IS '기록관ID';
COMMENT ON COLUMN TB_OPOPENGROUP.OPEN_SYS_CD IS '공개시스템코드 (01:원문정보공개시스템)';
COMMENT ON COLUMN TB_OPOPENGROUP.OPEN_DTIME IS '공개일시';
COMMENT ON COLUMN TB_OPOPENGROUP.CHOICE_CNT IS '기록물건 선정 수량';
COMMENT ON COLUMN TB_OPOPENGROUP.EXCEPT_CNT IS '기록물건 제외 수량';
COMMENT ON COLUMN TB_OPOPENGROUP.APPEND_CNT IS '기록물건 추가 수량';
COMMENT ON COLUMN TB_OPOPENGROUP.LIST_TYPE_CD IS '[OP02] 목록구분코드 (01:신규목록, 02:공개재분류, 03:재평가, 04:기관간인계, 05:이관, 06:폐기)';
COMMENT ON COLUMN TB_OPOPENGROUP.LIST_PROV_FLAG IS '목록제공여부 (0:미제공, 1:제공)';
COMMENT ON COLUMN TB_OPOPENGROUP.LIST_PROV_DTIME IS '목록제공일시';
COMMENT ON COLUMN TB_OPOPENGROUP.REG_DTIME IS '등록일시';

CREATE INDEX TB_OPOPENGROUP_IX01 ON TB_OPOPENGROUP
(RECORD_CENTER_ID, OPEN_SYS_CD, OPEN_DTIME)
LOGGING
TABLESPACE TS_OPIDX01
NOPARALLEL;

-- TB_OPOPENLIST

CREATE TABLE TB_OPOPENLIST
(
  RECORD_CENTER_ID   VARCHAR2(7 BYTE)               NOT NULL,
  OPEN_SYS_CD        VARCHAR2(2 BYTE)               NOT NULL,
  OPEN_STATE         VARCHAR2(1 BYTE)   DEFAULT '0' NOT NULL,
  OPEN_DTIME         VARCHAR2(14 BYTE)                  NULL,
  ORG_CD             VARCHAR2(7 BYTE)               NOT NULL,
  FOLDER_ID          VARCHAR2(14 BYTE)              NOT NULL,
  RECORD_ID          VARCHAR2(14 BYTE)              NOT NULL,
  CREAT_SYS_CD       VARCHAR2(2 BYTE)               NOT NULL,
  CREAT_YYYY         VARCHAR2(4 BYTE)               NOT NULL,
  REG_DTIME          VARCHAR2(14 BYTE)              NOT NULL
)
TABLESPACE TS_OPDAT01
LOGGING
NOCACHE
NOPARALLEL;

COMMENT ON TABLE TB_OPOPENLIST IS '공개목록';
COMMENT ON COLUMN TB_OPOPENLIST.RECORD_CENTER_ID IS '기록관ID';
COMMENT ON COLUMN TB_OPOPENLIST.OPEN_SYS_CD IS '공개시스템코드 (01:원문정보공개시스템)';
COMMENT ON COLUMN TB_OPOPENLIST.OPEN_STATE IS '[OP01] 공개상태 (0:미선정, 1:제외, 2:선정, 3:추가, 4:확정(선정), 5:확정(제외))';
COMMENT ON COLUMN TB_OPOPENLIST.OPEN_DTIME IS '공개일시 (공개확정일시)';
COMMENT ON COLUMN TB_OPOPENLIST.ORG_CD IS '처리과코드 (기록물철 생산부서)';
COMMENT ON COLUMN TB_OPOPENLIST.FOLDER_ID IS '기록물철ID';
COMMENT ON COLUMN TB_OPOPENLIST.RECORD_ID IS '기록물건ID';
COMMENT ON COLUMN TB_OPOPENLIST.CREAT_SYS_CD IS '생산시스템코드';
COMMENT ON COLUMN TB_OPOPENLIST.CREAT_YYYY IS '생산년도';
COMMENT ON COLUMN TB_OPOPENLIST.REG_DTIME IS '등록일시';

CREATE INDEX TB_OPOPENLIST_IX01 ON TB_OPOPENLIST
(RECORD_CENTER_ID, OPEN_SYS_CD, OPEN_STATE)
LOGGING
TABLESPACE TS_OPIDX01
NOPARALLEL;

CREATE INDEX TB_OPOPENLIST_IX02 ON TB_OPOPENLIST
(RECORD_CENTER_ID, OPEN_SYS_CD, ORG_CD, FOLDER_ID, RECORD_ID)
LOGGING
TABLESPACE TS_OPIDX01
NOPARALLEL;

CREATE INDEX TB_OPOPENLIST_IX03 ON TB_OPOPENLIST
(RECORD_CENTER_ID, OPEN_SYS_CD, OPEN_DTIME)
LOGGING
TABLESPACE TS_OPIDX01
NOPARALLEL;

-- TB_OPOPENLISTCHNG

CREATE TABLE TB_OPOPENLISTCHNG
(
  RECORD_CENTER_ID   VARCHAR2(7 BYTE)               NOT NULL,
  OPEN_SYS_CD        VARCHAR2(2 BYTE)               NOT NULL,
  OPEN_DTIME         VARCHAR2(14 BYTE)              NULL,
  LIST_TYPE_CD       VARCHAR2(2 BYTE)               NOT NULL,
  ORG_CD             VARCHAR2(7 BYTE)               NOT NULL,
  FOLDER_ID          VARCHAR2(14 BYTE)              NOT NULL,
  FOLDER_KEY         VARCHAR2(91 BYTE)              NOT NULL,
  RECORD_ID          VARCHAR2(14 BYTE)              NULL,
  RECORD_KEY         VARCHAR2(54 BYTE)              NULL,
  OPEN_GUBUN         VARCHAR2(10 BYTE)              NULL,
  PART_OPEN_RSN      VARCHAR2(500 BYTE)             NULL,
  PRESV_TERM_CD      VARCHAR2(2 BYTE)               NULL,
  TAKOVR_ORG_CD      VARCHAR2(7 BYTE)               NULL,
  TAKOVR_ORG_NM      VARCHAR2(256 BYTE)             NULL,
  TAKE_ORG_CD        VARCHAR2(7 BYTE)               NULL,
  TAKE_ORG_NM        VARCHAR2(256 BYTE)             NULL,
  DISUSE_YMD         VARCHAR2(8 BYTE)               NULL,
  TRANSF_YMD         VARCHAR2(8 BYTE)               NULL,
  CHNG_RSN           VARCHAR2(4000 BYTE)            NULL,
  REG_DTIME          VARCHAR2(14 BYTE)              NOT NULL
)
TABLESPACE TS_OPDAT01
LOGGING
NOCACHE
NOPARALLEL;

COMMENT ON TABLE TB_OPOPENLISTCHNG IS '공개목록 변경';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.RECORD_CENTER_ID IS '기록관ID';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.OPEN_SYS_CD IS '공개시스템코드 (01:원문정보공개시스템)';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.OPEN_DTIME IS '공개일시';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.LIST_TYPE_CD IS '[OP02] 목록구분코드 (01:신규목록, 02:공개재분류, 03:재평가, 04:기관간인계, 05:이관, 06:폐기)';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.ORG_CD IS '처리과코드';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.FOLDER_ID IS '기록물철ID';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.FOLDER_KEY IS '기록물철 식별자';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.RECORD_ID IS '기록물건ID';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.RECORD_KEY IS '기록물건 식별자';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.OPEN_GUBUN IS '공개구분 - 공개구분코드(1) + 비공개사유 1~8등급 YN값(8)';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.PART_OPEN_RSN IS '비공개사유';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.PRESV_TERM_CD IS '보존기간';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.TAKOVR_ORG_CD IS '인계처리과코드 (대표기관코드)';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.TAKOVR_ORG_NM IS '인계처리과명 (대표기관명)';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.TAKE_ORG_CD IS '인수처리과코드 (대표기관코드)';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.TAKE_ORG_NM IS '인수처리과명 (대표기관명)';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.DISUSE_YMD IS '폐기일자';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.TRANSF_YMD IS '이관일자';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.CHNG_RSN IS '변경사유';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.REG_DTIME IS '등록일시';

CREATE INDEX TB_OPOPENLISTCHNG_IX01 ON TB_OPOPENLISTCHNG
(RECORD_CENTER_ID, OPEN_SYS_CD, ORG_CD, FOLDER_ID, RECORD_ID)
LOGGING
TABLESPACE TS_OPIDX01
NOPARALLEL;

CREATE INDEX TB_OPOPENLISTCHNG_IX02 ON TB_OPOPENLISTCHNG
(RECORD_CENTER_ID, OPEN_SYS_CD, OPEN_DTIME)
LOGGING
TABLESPACE TS_OPIDX01
NOPARALLEL;

CREATE INDEX TB_OPOPENLISTCHNG_IX03 ON TB_OPOPENLISTCHNG
(RECORD_CENTER_ID, OPEN_SYS_CD, LIST_TYPE_CD)
LOGGING
TABLESPACE TS_OPIDX01
NOPARALLEL;

-- TB_OPORIGNPROVHIST

CREATE TABLE TB_OPORIGNPROVHIST
(
  RECORD_CENTER_ID   VARCHAR2(7 BYTE)               NOT NULL,
  OPEN_SYS_CD        VARCHAR2(2 BYTE)               NOT NULL,
  ORG_CD             VARCHAR2(7 BYTE)               NOT NULL,
  FOLDER_ID          VARCHAR2(14 BYTE)              NOT NULL,
  RECORD_ID          VARCHAR2(14 BYTE)              NOT NULL,
  ORIGN_FILE_ID      VARCHAR2(19 BYTE)              NOT NULL,
  REQ_DTIME          VARCHAR2(14 BYTE)              NOT NULL,
  REG_DTIME          VARCHAR2(14 BYTE)              NOT NULL
)
TABLESPACE TS_OPDAT01
LOGGING
NOCACHE
NOPARALLEL;

COMMENT ON TABLE TB_OPORIGNPROVHIST IS '원문제공이력';
COMMENT ON COLUMN TB_OPORIGNPROVHIST.RECORD_CENTER_ID IS '기록관ID';
COMMENT ON COLUMN TB_OPORIGNPROVHIST.OPEN_SYS_CD IS '공개시스템코드 (01:원문정보공개시스템)';
COMMENT ON COLUMN TB_OPORIGNPROVHIST.ORG_CD IS '처리과코드';
COMMENT ON COLUMN TB_OPORIGNPROVHIST.FOLDER_ID IS '기록물철ID';
COMMENT ON COLUMN TB_OPORIGNPROVHIST.RECORD_ID IS '기록물건ID';
COMMENT ON COLUMN TB_OPORIGNPROVHIST.ORIGN_FILE_ID IS '원문파일ID';
COMMENT ON COLUMN TB_OPORIGNPROVHIST.REQ_DTIME IS '요청일시';
COMMENT ON COLUMN TB_OPORIGNPROVHIST.REG_DTIME IS '등록일시';

CREATE INDEX TB_OPORIGNPROVHIST_IX01 ON TB_OPORIGNPROVHIST
(RECORD_CENTER_ID, OPEN_SYS_CD, ORG_CD, FOLDER_ID, RECORD_ID, ORIGN_FILE_ID)
LOGGING
TABLESPACE TS_OPIDX01
NOPARALLEL;

-- SYNONYM, GRANT 추가 (RMSUSR01)
CREATE OR REPLACE SYNONYM RMSUSR01.TB_OPOPENLIST FOR TB_OPOPENLIST;
CREATE OR REPLACE SYNONYM RMSUSR01.TB_OPOPENGROUP FOR TB_OPOPENGROUP;
CREATE OR REPLACE SYNONYM RMSUSR01.TB_OPOPENLISTCHNG FOR TB_OPOPENLISTCHNG;
CREATE OR REPLACE SYNONYM RMSUSR01.TB_OPORIGNPROVHIST FOR TB_OPORIGNPROVHIST;

GRANT SELECT, INSERT, UPDATE, DELETE ON TB_OPOPENLIST TO RMSUSR01;
GRANT SELECT, INSERT, UPDATE, DELETE ON TB_OPOPENGROUP TO RMSUSR01;
GRANT SELECT, INSERT, UPDATE, DELETE ON TB_OPOPENLISTCHNG TO RMSUSR01;
GRANT SELECT, INSERT, UPDATE, DELETE ON TB_OPORIGNPROVHIST TO RMSUSR01;


-- SYNONYM, GRANT 추가 (RMSOPEN01)
CREATE OR REPLACE SYNONYM RMSOPEN01.TB_OPOPENGROUP FOR TB_OPOPENGROUP;
CREATE OR REPLACE SYNONYM RMSOPEN01.TB_OPOPENLIST FOR TB_OPOPENLIST;
CREATE OR REPLACE SYNONYM RMSOPEN01.TB_OPOPENLISTCHNG FOR TB_OPOPENLISTCHNG;
CREATE OR REPLACE SYNONYM RMSOPEN01.TB_OPORIGNPROVHIST FOR TB_OPORIGNPROVHIST;
CREATE OR REPLACE SYNONYM RMSOPEN01.TB_STORIGNFILE FOR TB_STORIGNFILE;
CREATE OR REPLACE SYNONYM RMSOPEN01.TB_RDRECORD FOR TB_RDRECORD;
CREATE OR REPLACE SYNONYM RMSOPEN01.TB_RDFOLDER FOR TB_RDFOLDER; 

GRANT SELECT ON TB_OPOPENLIST TO RMSOPEN01;
GRANT SELECT ON TB_RDFOLDER TO RMSOPEN01;
GRANT SELECT ON TB_RDRECORD TO RMSOPEN01;
GRANT SELECT ON TB_STORIGNFILE TO RMSOPEN01;
GRANT SELECT, INSERT ON TB_OPORIGNPROVHIST TO RMSOPEN01;


------------------------------------------------------------------------------
-- 통합정보공개시스템 연계화면 메뉴추가
------------------------------------------------------------------------------

--TB_STMENULINK 
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM)
 Values (451, '3', '원문정보공개시스템');
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM, LINK_URL)
 Values (452, '4', '정보공개대상선정', 'open/open/navigateDiosChoiceOrgPagedList.rms');
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM, LINK_URL)
 Values (453, '4', '정보공개대상추가', 'open/open/navigateDiosAppendOrgPagedList.rms');
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM, LINK_URL)
 Values (454, '4', '정보공개대상확정', 'open/open/navigateDiosConfirmPagedList.rms');
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM, LINK_URL)
 Values (455, '4', '정보공개현황', 'open/open/navigateDiosCurStatPagedList.rms');
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM, LINK_URL)
 Values (456, '4', '원문서비스현황', 'open/open/navigateDiosOrignOrgPagedList.rms');

--TB_STMENU 
Insert into TB_STMENU (RECORD_CENTER_ID, MENU_ID, MENU_NM, UPPER_MENU_ID, MENU_DIV_CD, MENU_SORT_SNO, USE_FLAG, MENU_LINK_SNO, WORK_DTIME)
(
    select record_center_id, '451', '원문정보공개시스템', '48', '3', 1, '1', 451, TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
    from tb_strecordcenter A
    where not exists (
        select 1
        from tb_stmenu B
        where A.record_center_id = B.record_center_id
        AND B.menu_id = '451'
    )
);



Insert into TB_STMENU (RECORD_CENTER_ID, MENU_ID, MENU_NM, UPPER_MENU_ID, MENU_DIV_CD, MENU_SORT_SNO, USE_FLAG, MENU_LINK_SNO, MENU_GRANT_LEVL, WORK_DTIME)
(
    select record_center_id, '452', '정보공개대상선정', '451', '4', 1, '1', 452, '3', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
    from tb_strecordcenter A
    where not exists (
        select 1
        from tb_stmenu B
        where A.record_center_id = B.record_center_id
        AND B.menu_id = '452'
    )
);




Insert into TB_STMENU (RECORD_CENTER_ID, MENU_ID, MENU_NM, UPPER_MENU_ID, MENU_DIV_CD, MENU_SORT_SNO, USE_FLAG, MENU_LINK_SNO, MENU_GRANT_LEVL, WORK_DTIME)
(
    select record_center_id, '453', '정보공개대상추가', '451', '4', 3, '1', 453, '3', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
    from tb_strecordcenter A
    where not exists (
        select 1
        from tb_stmenu B
        where A.record_center_id = B.record_center_id
        AND B.menu_id = '453'
    )
);


Insert into TB_STMENU (RECORD_CENTER_ID, MENU_ID, MENU_NM, UPPER_MENU_ID, MENU_DIV_CD, MENU_SORT_SNO, USE_FLAG, MENU_LINK_SNO, MENU_GRANT_LEVL, WORK_DTIME)
(
    select record_center_id, '454', '정보공개대상확정', '451', '4', 4, '1', 454, '3', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
    from tb_strecordcenter A
    where not exists (
        select 1
        from tb_stmenu B
        where A.record_center_id = B.record_center_id
        AND B.menu_id = '454'
    )
);


Insert into TB_STMENU (RECORD_CENTER_ID, MENU_ID, MENU_NM, UPPER_MENU_ID, MENU_DIV_CD, MENU_SORT_SNO, USE_FLAG, MENU_LINK_SNO, MENU_GRANT_LEVL, WORK_DTIME)
(
    select record_center_id, '455', '정보공개현황', '451', '4', 5, '1', 455, '3', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
    from tb_strecordcenter A
    where not exists (
        select 1
        from tb_stmenu B
        where A.record_center_id = B.record_center_id
        AND B.menu_id = '455'
    )
);


Insert into TB_STMENU (RECORD_CENTER_ID, MENU_ID, MENU_NM, UPPER_MENU_ID, MENU_DIV_CD, MENU_SORT_SNO, USE_FLAG, MENU_LINK_SNO, MENU_GRANT_LEVL, WORK_DTIME)
(
    select record_center_id, '456', '원문서비스현황', '451', '4', 6, '1', 456, '3', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
    from tb_strecordcenter A
    where not exists (
        select 1
        from tb_stmenu B
        where A.record_center_id = B.record_center_id
        AND B.menu_id = '456'
    )
);

--TB_STMENUGRANTLINK 
Insert into TB_STMENUGRANTLINK (RECORD_CENTER_ID, MENU_GRANT_GRP_CD, MENU_ID, WORK_DTIME)
(
    select record_center_id, '003', '452', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
    from tb_strecordcenter A
    where not exists (
        select 1 
        from tb_stmenugrantlink B
        where a.record_center_id = B.record_center_id
        and B.menu_grant_grp_cd = '003'
        and B.menu_id = '452'
    )
);



Insert into TB_STMENUGRANTLINK (RECORD_CENTER_ID, MENU_GRANT_GRP_CD, MENU_ID, WORK_DTIME)
(
    select record_center_id, '003', '453', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
    from tb_strecordcenter A
    where not exists (
        select 1 
        from tb_stmenugrantlink B
        where a.record_center_id = B.record_center_id
        and B.menu_grant_grp_cd = '003'
        and B.menu_id = '453'
    )
);


Insert into TB_STMENUGRANTLINK (RECORD_CENTER_ID, MENU_GRANT_GRP_CD, MENU_ID, WORK_DTIME)
(
    select record_center_id, '003', '454', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
    from tb_strecordcenter A
    where not exists (
        select 1 
        from tb_stmenugrantlink B
        where a.record_center_id = B.record_center_id
        and B.menu_grant_grp_cd = '003'
        and B.menu_id = '454'
    )
);


Insert into TB_STMENUGRANTLINK (RECORD_CENTER_ID, MENU_GRANT_GRP_CD, MENU_ID, WORK_DTIME)
(
    select record_center_id, '003', '455', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
    from tb_strecordcenter A
    where not exists (
        select 1 
        from tb_stmenugrantlink B
        where a.record_center_id = B.record_center_id
        and B.menu_grant_grp_cd = '003'
        and B.menu_id = '455'
    )
);


Insert into TB_STMENUGRANTLINK (RECORD_CENTER_ID, MENU_GRANT_GRP_CD, MENU_ID, WORK_DTIME)
(
    select record_center_id, '003', '456', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
    from tb_strecordcenter A
    where not exists (
        select 1 
        from tb_stmenugrantlink B
        where a.record_center_id = B.record_center_id
        and B.menu_grant_grp_cd = '003'
        and B.menu_id = '456'
    )
);

--메뉴 순서 변경
UPDATE TB_STMENU SET MENU_SORT_SNO = 2, MENU_NM = '기록물통합검색시스템' WHERE MENU_ID = '162';

UPDATE TB_STMENU SET USE_FLAG = '0' WHERE MENU_ID IN ('414','415','416');
--메뉴권한그룹 수정
DELETE FROM TB_STMENUGRANTLINK WHERE MENU_ID IN ('414','415','416');

------------------------------------------------------------------------------
-- 시스템관리 모니터링 메뉴 제거
------------------------------------------------------------------------------

UPDATE TB_STMENU SET USE_FLAG = '0' WHERE MENU_ID IN ('47','160','161','410','411','412','413');

DELETE FROM TB_STMENUGRANTLINK WHERE MENU_ID IN ('410','411','412','413');

------------------------------------------------------------------------------
-- 통합정보공개시스템 공통코드 추가
------------------------------------------------------------------------------

INSERT INTO TB_ZZCOMTYPECD (COM_TYPE_CD, COM_TYPE_CD_NM, DESCR, USE_FLAG, REG_DTIME)
VALUES ('OP01','공개상태','공개진행상태', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'))
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP01','0','미선정','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP01','1','선정','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP01','2','제외','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP01','3','추가','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP01','4','확정(선정)','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP01','5','확정(제외)','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMTYPECD (COM_TYPE_CD, COM_TYPE_CD_NM, DESCR, USE_FLAG, REG_DTIME)
VALUES ('OP02','공개목록유형','정보공개목록 제공 유형', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'))
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP02','01','신규목록','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP02','02','공개재분류','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP02','03','평가','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP02','04','폐기','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP02','05','이관','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP02','06','기관간인계','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

------------------------------------------------------------------------------
-- 철/건검색 질의저장 컬럼 추가
------------------------------------------------------------------------------

ALTER TABLE TB_STFOLDERQUERY MODIFY FOLDER_TITLE VARCHAR2(500 BYTE);

ALTER TABLE TB_STFOLDERQUERY ADD INNER_TITLE VARCHAR2(500 BYTE);

COMMENT ON COLUMN TB_STFOLDERQUERY.INNER_TITLE IS '기록물철제목 결과내검색';

ALTER TABLE TB_STFOLDERQUERY ADD SEARCH_DIV_CD VARCHAR2(1 BYTE);

COMMENT ON COLUMN TB_STFOLDERQUERY.SEARCH_DIV_CD IS '처리과검색구분 (1:처리부서,2:생산부서)';

ALTER TABLE TB_STRECORDQUERY ADD INNER_TITLE VARCHAR2(500 BYTE);

COMMENT ON COLUMN TB_STRECORDQUERY.INNER_TITLE IS '기록물건제목 결과내검색';

ALTER TABLE TB_STRECORDQUERY ADD SEARCH_DIV_CD VARCHAR2(1 BYTE);

COMMENT ON COLUMN TB_STRECORDQUERY.SEARCH_DIV_CD IS '처리과검색구분 (1:처리부서,2:생산부서)';

------------------------------------------------------------------------------
-- 기관간인수 에러 저장용 시퀀스 추가
------------------------------------------------------------------------------

CREATE SEQUENCE TB_DFTAKEERROR_SQ01
MINVALUE 0
MAXVALUE 999999999999999999999999999
START WITH 1
INCREMENT BY 1
NOCACHE;

--필요한 경우 시노님 생성 및 권한부여 부분 주석 해제 후 실행
CREATE SYNONYM RMSUSR01.TB_DFTAKEERROR_SQ01 FOR TB_DFTAKEERROR_SQ01;

GRANT SELECT ON TB_DFTAKEERROR_SQ01 TO RMSUSR01;

------------------------------------------------------------------------------
-- 접근범위재분류 의견등록 테이블 org_cd 컬럼유형 수정 (접근범위재분류 CHAR(7) -> VARCHAR2(7) 로 변경)
------------------------------------------------------------------------------

ALTER TABLE TB_RDRANGERESORTOPIN MODIFY ORG_CD VARCHAR2(7) DEFAULT '0';

UPDATE TB_RDRANGERESORTOPIN SET ORG_CD='0' WHERE ORG_CD='0      ';

------------------------------------------------------------------------------
-- 이관파일생성건목록 테이블 컬럼사이즈 수정 (이관파일 생성 시 컬럼에 들어가는 값이 '-' 구분자가 포함되기 때문에 54 byte 로 수정)
------------------------------------------------------------------------------

ALTER TABLE TB_DFTRANSFSTORERCD MODIFY RECORD_NEO_SND_FILE_NM VARCHAR2(54);


-----------------------------------------------------------------------------
-- 시스템관리 > 환경설정 > 포맷변환서버관리 > 파일확장자관리
-- 변환가능여부 코드 없을 경우 추가
------------------------------------------------------------------------------

INSERT INTO TB_ZZCOMTYPECD (
    COM_TYPE_CD, COM_TYPE_CD_NM, DESCR, USE_FLAG, REG_DTIME
) VALUES (
    'ST29', '변환가능여부', '포맷변환가능 확장자 여부', '1', '20141201090000'
);

INSERT INTO TB_ZZCOMCD (
    COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD
) VALUES (
    'ST29', '1', '변환가능', '', '1', '20141201090000', ''
);

INSERT INTO TB_ZZCOMCD (
    COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD
) VALUES (
    'ST29', '2', '변환불가', '', '1', '20141201090000', ''
);


------------------------------------------------------------------------------
-- rms2014 패치 완료 (rms_ver 및 db_ver 은 바뀔 수 있음)
------------------------------------------------------------------------------

INSERT INTO RMS_HISTORY
( seq, rms_ver, db_ver, cont, reg_dtime )
VALUES
( 
    NVL((SELECT MAX(NVL(seq,0))+1 FROM RMS_HISTORY),0)
    , '2.51.00'
    , '00_20150831_00'
    , 'RMS 2.0 패치'
    , TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
);

commit;
