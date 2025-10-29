set echo on
spool /data/oracle/admin/RMSORA9I/logs/99_CreateDBMSJOB.log

variable jobno number;
EXEC DBMS_JOB.SUBMIT(:jobno, 'RMSDBA01.SP_RESET_SEQ;' , SYSDATE , 'trunc(to_date(to_char(sysdate,''YYYY'')+1,''YYYY''),''YYYY'')');

variable jobno number;
EXEC DBMS_JOB.SUBMIT(:jobno, 'DBMS_STATS.GATHER_SCHEMA_STATS(''RMSDBA01'' , CASCADE => TRUE);' , SYSDATE , 'NEXT_DAY(TRUNC(SYSDATE),7)+1/24');

commit;

--JOB NUMBER가 상이할 수 있음.
EXEC DBMS_JOB.RUN(1);
EXEC DBMS_JOB.RUN(2);
spool off
