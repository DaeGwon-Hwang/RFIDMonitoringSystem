set echo on
spool /data/oracle/admin/RMSORA9I/logs/12_InsertRecordCenter.log
/* *****************************************
 * ��ϰ�ȯ�漳�� ������ �Է�
 * *****************************************
 */

/* *****************************************
 * 1. ��ϰ�����
 * - RECORD_CENTER_CD : ��ϰ��ڵ�
 * - RECORD_CENTER_nm : ��ϰ���
 * - REP_ORG_CD : ��ǥ����ڵ�
 * - REP_ORG_NM : ��ǥ�����
 * - TEL_NO : ��ϰ� ����� ����ó
 * *****************************************
 */
INSERT INTO TB_STRECORDCENTER (RECORD_CENTER_ID, RECORD_CENTER_CD, RECORD_CENTER_NM, REP_ORG_CD, REP_ORG_NM, TEL_NO, REG_DTIME)
VALUES ('0000001','0000000','������Ͽ�','0000000','������ġ�� ������Ͽ�','000-000-0000',TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS'));


/* *****************************************
 * 2. ������
 * *****************************************
 */
INSERT INTO TB_STLINKTRGT (LINK_TRGT_ID, LINK_TRGT_NM,CREAT_SYS_CD) VALUES ('L000001', '�⺻', '01');

commit;


spool off 
