
-- ����û ��ġ�ÿ��� �����Ͽ��� �մϴ�.



-- 1. ����û�� �ڵ� ��� 
UPDATE TB_ZZCOMTYPECD SET USE_FLAG='1' WHERE COM_TYPE_CD IN ('RD65','RD66');

-- �������� ��� 
UPDATE TB_ZZCOMCD SET USE_FLAG='1' WHERE COM_TYPE_CD='RD65' AND COM_CD='11';

-- ��Ȱ��Ϻ� ��� 
UPDATE TB_ZZCOMCD SET USE_FLAG='1' WHERE COM_TYPE_CD='RD66' AND COM_CD='21';

-- �λ�ī�� ��� 
UPDATE TB_ZZCOMCD SET USE_FLAG='1' WHERE COM_TYPE_CD='RD66' AND COM_CD='22';



-- 2.����û ���� �޴� ��� 
UPDATE TB_STMENU SET USE_FLAG='1' WHERE MENU_ID IN ('18','62','63','202','203','204');


--����û ��ġ�ÿ��� ROLLBACK �� COMMIT; �� ���� 
rollback;
--commit;
