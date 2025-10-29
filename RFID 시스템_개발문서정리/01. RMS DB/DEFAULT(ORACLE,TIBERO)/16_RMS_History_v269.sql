
INSERT INTO RMS_HISTORY
( seq, rms_ver, db_ver, cont, reg_dtime )
VALUES
( 
    NVL((SELECT MAX(NVL(seq,0))+1 FROM RMS_HISTORY),0)
    , '2.68.00'
    , '00_20210101'
    , '기록관리시스템 설치'
    , TO_CHAR(SYSDATE,'YYYYMMDDHH24MISS')
);

commit;
