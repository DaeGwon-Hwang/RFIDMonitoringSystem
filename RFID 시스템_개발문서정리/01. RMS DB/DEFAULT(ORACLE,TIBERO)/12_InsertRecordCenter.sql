set echo on
spool /data/oracle/admin/RMSORA9I/logs/12_InsertRecordCenter.log
/* *****************************************
 * 기록관환경설정 데이터 입력
 * *****************************************
 */

/* *****************************************
 * 1. 기록관지정
 * - RECORD_CENTER_CD : 기록관코드
 * - RECORD_CENTER_nm : 기록관명
 * - REP_ORG_CD : 대표기관코드
 * - REP_ORG_NM : 대표기관명
 * - TEL_NO : 기록관 담당자 연락처
 * *****************************************
 */
INSERT INTO TB_STRECORDCENTER (RECORD_CENTER_ID, RECORD_CENTER_CD, RECORD_CENTER_NM, REP_ORG_CD, REP_ORG_NM, TEL_NO, REG_DTIME)
VALUES ('0000001','0000000','국가기록원','0000000','행정자치부 국가기록원','000-000-0000',TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'));


/* *****************************************
 * 2. 연계대상
 * *****************************************
 */
INSERT INTO TB_STLINKTRGT (LINK_TRGT_ID, LINK_TRGT_NM,CREAT_SYS_CD) VALUES ('L000001', '기본', '01');

commit;


spool off 
