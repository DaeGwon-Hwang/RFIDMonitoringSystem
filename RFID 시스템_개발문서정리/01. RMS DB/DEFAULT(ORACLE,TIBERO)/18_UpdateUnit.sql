
-------------------------------------------------------------
-- �������� ��ü������������� �����Ͽ��� �մϴ�.

-- 1. '������������' ȭ�� : ���
UPDATE TB_STMENU SET USE_FLAG='1' WHERE MENU_ID='320';

-- 2. '����������ûó��', '���������������ó��' ȭ�� : �̻��
UPDATE TB_STMENU SET USE_FLAG='0' WHERE MENU_ID='324';
UPDATE TB_STMENU SET USE_FLAG='0' WHERE MENU_ID='325';


--�������� ��ü����������� ROLLBACK �� COMMIT; �� ���� 
rollback;
--commit;
