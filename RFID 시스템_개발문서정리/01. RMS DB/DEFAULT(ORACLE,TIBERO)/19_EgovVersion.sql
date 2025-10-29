-- 289, 290������ dbf ���� ��ġ �����Ͽ� ���� ����
-- tibero�� ���  289, 290������ LOGGING �ɼ��� �����Ͻð� ����


------------------------------------------------------------------------------
-- ���� ���̺� ��� 
------------------------------------------------------------------------------
CREATE TABLE TB_ZZREQEXCELLIST_TEMP AS SELECT * FROM TB_ZZREQEXCELLIST;

CREATE TABLE TB_STMENU_TEMP AS SELECT * FROM TB_STMENU;

CREATE TABLE TB_STMENULINK_TEMP AS SELECT * FROM TB_STMENULINK;

CREATE TABLE TB_STMENUGRANTLINK_TEMP AS SELECT * FROM TB_STMENUGRANTLINK;

CREATE TABLE TB_STFOLDERQUERY_TEMP AS SELECT * FROM TB_STFOLDERQUERY;

CREATE TABLE TB_STRECORDQUERY_TEMP AS SELECT * FROM TB_STRECORDQUERY;



------------------------------------------------------------------------------
-- ���� ���� ���� ���濡 ���� TABLE ����
------------------------------------------------------------------------------

-- ������ ���� ������ ��ü ��θ� ���� �ϴ� �÷� ����
ALTER TABLE TB_ZZREQEXCELLIST DROP(FULL_PATH);

-- ������ ���� ������ KEY �� ���� �ϴ� �÷� �߰�
ALTER TABLE TB_ZZREQEXCELLIST ADD(FILE_KEY VARCHAR2(100));

COMMENT ON COLUMN TB_ZZREQEXCELLIST.FILE_KEY IS '���� KEY';

-- ������ ���� ������ INDEX �� ���� �ϴ� �÷� �߰�
ALTER TABLE TB_ZZREQEXCELLIST ADD(REQ_INDEX VARCHAR2(1));

COMMENT ON COLUMN TB_ZZREQEXCELLIST.REQ_INDEX IS 'INDEX';

-- ��ü ���� ��ġ�� ���Ͽ� �ڷ������ �°� �÷� Ÿ�� ����
ALTER TABLE TB_ZZREQEXCELLIST MODIFY(FILE_NM VARCHAR2(500));


------------------------------------------------------------------------------
-- ���� ���� ���� TABLE �߰�
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

COMMENT ON TABLE TB_STFILEMNG IS '���ϰ���';

COMMENT ON COLUMN TB_STFILEMNG.FILE_KEY IS '����KEY';

COMMENT ON COLUMN TB_STFILEMNG.FILE_NM IS '���ϸ�';

COMMENT ON COLUMN TB_STFILEMNG.FILE_SIZE IS '���Ͽ뷮';

COMMENT ON COLUMN TB_STFILEMNG.FILE_PATH IS '���ϰ��(��ȣȭ)';

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


--�ʿ��� ��� �ó�� ���� �� ���Ѻο� �κ� �ּ� ���� �� ����


CREATE OR REPLACE SYNONYM RMSUSR01.TB_STFILEMNG FOR TB_STFILEMNG;

GRANT DELETE, INSERT, SELECT, UPDATE ON TB_STFILEMNG TO RMSUSR01;


------------------------------------------------------------------------------
-- ID GEN egov ����������Ʈ �� TABLE �߰�
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


COMMENT ON COLUMN COMTECOPSEQ.TABLE_NAME IS '���̺��';

COMMENT ON COLUMN COMTECOPSEQ.NEXT_ID IS '����ID';


ALTER TABLE COMTECOPSEQ ADD
(
    CONSTRAINT COMTECOPSEQ_PK PRIMARY KEY (TABLE_NAME)
    USING
        INDEX TABLESPACE TS_STIDX01
        PCTFREE 5
)
;

INSERT INTO COMTECOPSEQ ( TABLE_NAME, NEXT_ID ) VALUES ('RMS_FILE_ID', 1);

--�ʿ��� ��� �ó�� ���� �� ���Ѻο� �κ� �ּ� ���� �� ����


CREATE OR REPLACE SYNONYM RMSUSR01.COMTECOPSEQ FOR COMTECOPSEQ;

GRANT DELETE, INSERT, SELECT, UPDATE ON COMTECOPSEQ TO RMSUSR01;


------------------------------------------------------------------------------
-- ��⺸������ ������ ������ �̷� TABLE �߰�
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

COMMENT ON TABLE TB_STFORMATVERIFYHIST IS '��⺸������ ������ ������ �̷�';

COMMENT ON COLUMN TB_STFORMATVERIFYHIST.RECORD_CENTER_ID IS '��ϰ�ID';

COMMENT ON COLUMN TB_STFORMATVERIFYHIST.NEO_GUBUN IS 'NEO���� (1:ö,2:��)';

COMMENT ON COLUMN TB_STFORMATVERIFYHIST.TRGT_ID IS '����Ϲ�ID (��Ϲ�öID/��Ϲ���ID)';

COMMENT ON COLUMN TB_STFORMATVERIFYHIST.REQ_USER_ID IS '��û��ID';

COMMENT ON COLUMN TB_STFORMATVERIFYHIST.REQ_DTIME IS '��û�Ͻ�';

COMMENT ON COLUMN TB_STFORMATVERIFYHIST.RESULT_DIV_CD IS '������� (1:����,2:����)';

COMMENT ON COLUMN TB_STFORMATVERIFYHIST.RESULT_CODE IS '����ڵ�';

COMMENT ON COLUMN TB_STFORMATVERIFYHIST.RESULT_MSG IS '����޽���';


--�ó�� ���� �� ���Ѻο� �κ� �ּ� ���� �� ����
CREATE OR REPLACE SYNONYM RMSUSR01.TB_STFORMATVERIFYHIST FOR TB_STFORMATVERIFYHIST;

GRANT DELETE, INSERT, SELECT, UPDATE ON TB_STFORMATVERIFYHIST TO RMSUSR01;



------------------------------------------------------------------------------
-- ��⺸������ ������Ȳ �޴� �߰� ��ũ��Ʈ
------------------------------------------------------------------------------

--TB_STMENULINK 
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM, LINK_IMG_LEFT, LINK_CSS)
 Values (447, '2', '������Ȳ', 'cm/left/left_title_ACCS03.gif', 'ACCS.css');
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM)
 Values (448, '3', '��⺸�����������Ȳ');
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM, LINK_URL)
 Values (449, '4', '�⵵��������Ȳ', 'navigateVerifyStatusByYearList.rms');
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM, LINK_URL)
 Values (450, '4', '��Ϲ���������Ȳ', 'navigateVerifyStatusByDocList.rms');

--TB_STMENU 
Insert into TB_STMENU (RECORD_CENTER_ID, MENU_ID, MENU_NM, UPPER_MENU_ID, MENU_DIV_CD, MENU_SORT_SNO, USE_FLAG, MENU_LINK_SNO, WORK_DTIME)
(
    select record_center_id, '447', '������Ȳ', '6', '2', 3, '1', 447, TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
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
    select record_center_id, '448', '��⺸�����������Ȳ', '447', '3', 1, '1', 448, TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
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
    select record_center_id, '450', '��Ϲ���������Ȳ', '448', '4', 2, '1', 450, '3', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
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
    select record_center_id, '449', '�⵵��������Ȳ', '448', '4', 1, '1', 449, '3', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
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
-- �������������ý��� ����� DB ����
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

COMMENT ON TABLE TB_OPOPENGROUP IS '������ȹ';
COMMENT ON COLUMN TB_OPOPENGROUP.RECORD_CENTER_ID IS '��ϰ�ID';
COMMENT ON COLUMN TB_OPOPENGROUP.OPEN_SYS_CD IS '�����ý����ڵ� (01:�������������ý���)';
COMMENT ON COLUMN TB_OPOPENGROUP.OPEN_DTIME IS '�����Ͻ�';
COMMENT ON COLUMN TB_OPOPENGROUP.CHOICE_CNT IS '��Ϲ��� ���� ����';
COMMENT ON COLUMN TB_OPOPENGROUP.EXCEPT_CNT IS '��Ϲ��� ���� ����';
COMMENT ON COLUMN TB_OPOPENGROUP.APPEND_CNT IS '��Ϲ��� �߰� ����';
COMMENT ON COLUMN TB_OPOPENGROUP.LIST_TYPE_CD IS '[OP02] ��ϱ����ڵ� (01:�űԸ��, 02:������з�, 03:����, 04:������ΰ�, 05:�̰�, 06:���)';
COMMENT ON COLUMN TB_OPOPENGROUP.LIST_PROV_FLAG IS '����������� (0:������, 1:����)';
COMMENT ON COLUMN TB_OPOPENGROUP.LIST_PROV_DTIME IS '��������Ͻ�';
COMMENT ON COLUMN TB_OPOPENGROUP.REG_DTIME IS '����Ͻ�';

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

COMMENT ON TABLE TB_OPOPENLIST IS '�������';
COMMENT ON COLUMN TB_OPOPENLIST.RECORD_CENTER_ID IS '��ϰ�ID';
COMMENT ON COLUMN TB_OPOPENLIST.OPEN_SYS_CD IS '�����ý����ڵ� (01:�������������ý���)';
COMMENT ON COLUMN TB_OPOPENLIST.OPEN_STATE IS '[OP01] �������� (0:�̼���, 1:����, 2:����, 3:�߰�, 4:Ȯ��(����), 5:Ȯ��(����))';
COMMENT ON COLUMN TB_OPOPENLIST.OPEN_DTIME IS '�����Ͻ� (����Ȯ���Ͻ�)';
COMMENT ON COLUMN TB_OPOPENLIST.ORG_CD IS 'ó�����ڵ� (��Ϲ�ö ����μ�)';
COMMENT ON COLUMN TB_OPOPENLIST.FOLDER_ID IS '��Ϲ�öID';
COMMENT ON COLUMN TB_OPOPENLIST.RECORD_ID IS '��Ϲ���ID';
COMMENT ON COLUMN TB_OPOPENLIST.CREAT_SYS_CD IS '����ý����ڵ�';
COMMENT ON COLUMN TB_OPOPENLIST.CREAT_YYYY IS '����⵵';
COMMENT ON COLUMN TB_OPOPENLIST.REG_DTIME IS '����Ͻ�';

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

COMMENT ON TABLE TB_OPOPENLISTCHNG IS '������� ����';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.RECORD_CENTER_ID IS '��ϰ�ID';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.OPEN_SYS_CD IS '�����ý����ڵ� (01:�������������ý���)';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.OPEN_DTIME IS '�����Ͻ�';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.LIST_TYPE_CD IS '[OP02] ��ϱ����ڵ� (01:�űԸ��, 02:������з�, 03:����, 04:������ΰ�, 05:�̰�, 06:���)';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.ORG_CD IS 'ó�����ڵ�';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.FOLDER_ID IS '��Ϲ�öID';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.FOLDER_KEY IS '��Ϲ�ö �ĺ���';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.RECORD_ID IS '��Ϲ���ID';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.RECORD_KEY IS '��Ϲ��� �ĺ���';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.OPEN_GUBUN IS '�������� - ���������ڵ�(1) + ��������� 1~8��� YN��(8)';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.PART_OPEN_RSN IS '���������';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.PRESV_TERM_CD IS '�����Ⱓ';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.TAKOVR_ORG_CD IS '�ΰ�ó�����ڵ� (��ǥ����ڵ�)';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.TAKOVR_ORG_NM IS '�ΰ�ó������ (��ǥ�����)';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.TAKE_ORG_CD IS '�μ�ó�����ڵ� (��ǥ����ڵ�)';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.TAKE_ORG_NM IS '�μ�ó������ (��ǥ�����)';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.DISUSE_YMD IS '�������';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.TRANSF_YMD IS '�̰�����';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.CHNG_RSN IS '�������';
COMMENT ON COLUMN TB_OPOPENLISTCHNG.REG_DTIME IS '����Ͻ�';

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

COMMENT ON TABLE TB_OPORIGNPROVHIST IS '���������̷�';
COMMENT ON COLUMN TB_OPORIGNPROVHIST.RECORD_CENTER_ID IS '��ϰ�ID';
COMMENT ON COLUMN TB_OPORIGNPROVHIST.OPEN_SYS_CD IS '�����ý����ڵ� (01:�������������ý���)';
COMMENT ON COLUMN TB_OPORIGNPROVHIST.ORG_CD IS 'ó�����ڵ�';
COMMENT ON COLUMN TB_OPORIGNPROVHIST.FOLDER_ID IS '��Ϲ�öID';
COMMENT ON COLUMN TB_OPORIGNPROVHIST.RECORD_ID IS '��Ϲ���ID';
COMMENT ON COLUMN TB_OPORIGNPROVHIST.ORIGN_FILE_ID IS '��������ID';
COMMENT ON COLUMN TB_OPORIGNPROVHIST.REQ_DTIME IS '��û�Ͻ�';
COMMENT ON COLUMN TB_OPORIGNPROVHIST.REG_DTIME IS '����Ͻ�';

CREATE INDEX TB_OPORIGNPROVHIST_IX01 ON TB_OPORIGNPROVHIST
(RECORD_CENTER_ID, OPEN_SYS_CD, ORG_CD, FOLDER_ID, RECORD_ID, ORIGN_FILE_ID)
LOGGING
TABLESPACE TS_OPIDX01
NOPARALLEL;

-- SYNONYM, GRANT �߰� (RMSUSR01)
CREATE OR REPLACE SYNONYM RMSUSR01.TB_OPOPENLIST FOR TB_OPOPENLIST;
CREATE OR REPLACE SYNONYM RMSUSR01.TB_OPOPENGROUP FOR TB_OPOPENGROUP;
CREATE OR REPLACE SYNONYM RMSUSR01.TB_OPOPENLISTCHNG FOR TB_OPOPENLISTCHNG;
CREATE OR REPLACE SYNONYM RMSUSR01.TB_OPORIGNPROVHIST FOR TB_OPORIGNPROVHIST;

GRANT SELECT, INSERT, UPDATE, DELETE ON TB_OPOPENLIST TO RMSUSR01;
GRANT SELECT, INSERT, UPDATE, DELETE ON TB_OPOPENGROUP TO RMSUSR01;
GRANT SELECT, INSERT, UPDATE, DELETE ON TB_OPOPENLISTCHNG TO RMSUSR01;
GRANT SELECT, INSERT, UPDATE, DELETE ON TB_OPORIGNPROVHIST TO RMSUSR01;


-- SYNONYM, GRANT �߰� (RMSOPEN01)
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
-- �������������ý��� ����ȭ�� �޴��߰�
------------------------------------------------------------------------------

--TB_STMENULINK 
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM)
 Values (451, '3', '�������������ý���');
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM, LINK_URL)
 Values (452, '4', '�������������', 'open/open/navigateDiosChoiceOrgPagedList.rms');
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM, LINK_URL)
 Values (453, '4', '������������߰�', 'open/open/navigateDiosAppendOrgPagedList.rms');
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM, LINK_URL)
 Values (454, '4', '�����������Ȯ��', 'open/open/navigateDiosConfirmPagedList.rms');
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM, LINK_URL)
 Values (455, '4', '����������Ȳ', 'open/open/navigateDiosCurStatPagedList.rms');
Insert into TB_STMENULINK (MENU_LINK_SNO, MENU_DIV_CD, LINK_NM, LINK_URL)
 Values (456, '4', '����������Ȳ', 'open/open/navigateDiosOrignOrgPagedList.rms');

--TB_STMENU 
Insert into TB_STMENU (RECORD_CENTER_ID, MENU_ID, MENU_NM, UPPER_MENU_ID, MENU_DIV_CD, MENU_SORT_SNO, USE_FLAG, MENU_LINK_SNO, WORK_DTIME)
(
    select record_center_id, '451', '�������������ý���', '48', '3', 1, '1', 451, TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
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
    select record_center_id, '452', '�������������', '451', '4', 1, '1', 452, '3', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
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
    select record_center_id, '453', '������������߰�', '451', '4', 3, '1', 453, '3', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
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
    select record_center_id, '454', '�����������Ȯ��', '451', '4', 4, '1', 454, '3', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
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
    select record_center_id, '455', '����������Ȳ', '451', '4', 5, '1', 455, '3', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
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
    select record_center_id, '456', '����������Ȳ', '451', '4', 6, '1', 456, '3', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
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

--�޴� ���� ����
UPDATE TB_STMENU SET MENU_SORT_SNO = 2, MENU_NM = '��Ϲ����հ˻��ý���' WHERE MENU_ID = '162';

UPDATE TB_STMENU SET USE_FLAG = '0' WHERE MENU_ID IN ('414','415','416');
--�޴����ѱ׷� ����
DELETE FROM TB_STMENUGRANTLINK WHERE MENU_ID IN ('414','415','416');

------------------------------------------------------------------------------
-- �ý��۰��� ����͸� �޴� ����
------------------------------------------------------------------------------

UPDATE TB_STMENU SET USE_FLAG = '0' WHERE MENU_ID IN ('47','160','161','410','411','412','413');

DELETE FROM TB_STMENUGRANTLINK WHERE MENU_ID IN ('410','411','412','413');

------------------------------------------------------------------------------
-- �������������ý��� �����ڵ� �߰�
------------------------------------------------------------------------------

INSERT INTO TB_ZZCOMTYPECD (COM_TYPE_CD, COM_TYPE_CD_NM, DESCR, USE_FLAG, REG_DTIME)
VALUES ('OP01','��������','�����������', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'))
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP01','0','�̼���','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP01','1','����','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP01','2','����','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP01','3','�߰�','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP01','4','Ȯ��(����)','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP01','5','Ȯ��(����)','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMTYPECD (COM_TYPE_CD, COM_TYPE_CD_NM, DESCR, USE_FLAG, REG_DTIME)
VALUES ('OP02','�����������','����������� ���� ����', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'))
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP02','01','�űԸ��','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP02','02','������з�','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP02','03','��','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP02','04','���','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP02','05','�̰�','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

INSERT INTO TB_ZZCOMCD (COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD) VALUES ('OP02','06','������ΰ�','', '1', TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'), '')
;

------------------------------------------------------------------------------
-- ö/�ǰ˻� �������� �÷� �߰�
------------------------------------------------------------------------------

ALTER TABLE TB_STFOLDERQUERY MODIFY FOLDER_TITLE VARCHAR2(500 BYTE);

ALTER TABLE TB_STFOLDERQUERY ADD INNER_TITLE VARCHAR2(500 BYTE);

COMMENT ON COLUMN TB_STFOLDERQUERY.INNER_TITLE IS '��Ϲ�ö���� ������˻�';

ALTER TABLE TB_STFOLDERQUERY ADD SEARCH_DIV_CD VARCHAR2(1 BYTE);

COMMENT ON COLUMN TB_STFOLDERQUERY.SEARCH_DIV_CD IS 'ó�����˻����� (1:ó���μ�,2:����μ�)';

ALTER TABLE TB_STRECORDQUERY ADD INNER_TITLE VARCHAR2(500 BYTE);

COMMENT ON COLUMN TB_STRECORDQUERY.INNER_TITLE IS '��Ϲ������� ������˻�';

ALTER TABLE TB_STRECORDQUERY ADD SEARCH_DIV_CD VARCHAR2(1 BYTE);

COMMENT ON COLUMN TB_STRECORDQUERY.SEARCH_DIV_CD IS 'ó�����˻����� (1:ó���μ�,2:����μ�)';

------------------------------------------------------------------------------
-- ������μ� ���� ����� ������ �߰�
------------------------------------------------------------------------------

CREATE SEQUENCE TB_DFTAKEERROR_SQ01
MINVALUE 0
MAXVALUE 999999999999999999999999999
START WITH 1
INCREMENT BY 1
NOCACHE;

--�ʿ��� ��� �ó�� ���� �� ���Ѻο� �κ� �ּ� ���� �� ����
CREATE SYNONYM RMSUSR01.TB_DFTAKEERROR_SQ01 FOR TB_DFTAKEERROR_SQ01;

GRANT SELECT ON TB_DFTAKEERROR_SQ01 TO RMSUSR01;

------------------------------------------------------------------------------
-- ���ٹ�����з� �ǰߵ�� ���̺� org_cd �÷����� ���� (���ٹ�����з� CHAR(7) -> VARCHAR2(7) �� ����)
------------------------------------------------------------------------------

ALTER TABLE TB_RDRANGERESORTOPIN MODIFY ORG_CD VARCHAR2(7) DEFAULT '0';

UPDATE TB_RDRANGERESORTOPIN SET ORG_CD='0' WHERE ORG_CD='0      ';

------------------------------------------------------------------------------
-- �̰����ϻ����Ǹ�� ���̺� �÷������� ���� (�̰����� ���� �� �÷��� ���� ���� '-' �����ڰ� ���ԵǱ� ������ 54 byte �� ����)
------------------------------------------------------------------------------

ALTER TABLE TB_DFTRANSFSTORERCD MODIFY RECORD_NEO_SND_FILE_NM VARCHAR2(54);


-----------------------------------------------------------------------------
-- �ý��۰��� > ȯ�漳�� > ���˺�ȯ�������� > ����Ȯ���ڰ���
-- ��ȯ���ɿ��� �ڵ� ���� ��� �߰�
------------------------------------------------------------------------------

INSERT INTO TB_ZZCOMTYPECD (
    COM_TYPE_CD, COM_TYPE_CD_NM, DESCR, USE_FLAG, REG_DTIME
) VALUES (
    'ST29', '��ȯ���ɿ���', '���˺�ȯ���� Ȯ���� ����', '1', '20141201090000'
);

INSERT INTO TB_ZZCOMCD (
    COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD
) VALUES (
    'ST29', '1', '��ȯ����', '', '1', '20141201090000', ''
);

INSERT INTO TB_ZZCOMCD (
    COM_TYPE_CD, COM_CD, COM_CD_NM, DESCR, USE_FLAG, REG_DTIME, SUB_COM_TYPE_CD
) VALUES (
    'ST29', '2', '��ȯ�Ұ�', '', '1', '20141201090000', ''
);


------------------------------------------------------------------------------
-- rms2014 ��ġ �Ϸ� (rms_ver �� db_ver �� �ٲ� �� ����)
------------------------------------------------------------------------------

INSERT INTO RMS_HISTORY
( seq, rms_ver, db_ver, cont, reg_dtime )
VALUES
( 
    NVL((SELECT MAX(NVL(seq,0))+1 FROM RMS_HISTORY),0)
    , '2.51.00'
    , '00_20150831_00'
    , 'RMS 2.0 ��ġ'
    , TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
);

commit;
