# Personelprj3D
 ---
 WASD 이동
 SPACE 점프
 E 상호작용
 TAB 인벤토리

 맵 곳곳에 있는 아이템을 활용하여 골대까지 도달하는 게임입니다.

 
## 트러블 슈팅
### 1.
3인칭 부분은 Dev - 3rdPersonView 브랜치에 존재합니다.
1인칭에서 3인칭으로 변경됨에 따라 기존 OnLookInput과 연결되어 작동하는 Interaction 스크립트도 수정해 주어야 했으나,
3인칭으로 변경뒤 수정해주지 못했음, 이렇게 해놓고 왜 안되지?? 하고 있었으나 원인은 찾게 되었습니다.

### 2. 
이번 개인 프로젝트는 시간대비 효율이 그렇게 좋지 못했던 프로젝트였습니다. 
수준별 주차를 복습하며 어떤기능이 어떻게 쓰이는지, 공부하는 주차라고 생각합니다.
물리적인 상호작용 (Collision)과 레이캐스트의 활용 여부가 중요하다고 생각되는데
생각보다 그렇게 많이 써 먹지 못했던것 같습니다. 사다리 등 여러 상호작용이 가능한 물체를 만들고 싶었지만,
제 한계는 사다리 뿐이었습니다. 사다리 조차 생각한 내용과 결과물이 달라 시간을 오래 잡아먹은 기능이었는데
생각의 순서가 잘못되어 문제를 해결 하는 것이 오래되었습니다.

기존:
사다리 = 상호작용이 가능해야함
위 아래로 움직일 수 있어야 하고 중력에 영향을 받지않음, 범위를 벗어나면 해당기능이 작동하지 않음
istrigger를 사용하여 플레이어가 충돌시에 위로 올라 갈 수 있게 작동 >> 목표인 레이캐스트와, forcemode가 사용되지 않음

개선:?
상호작용이 굳이 E를 눌러서 되야하는가?
플레이어가 사다리와 충돌시에 플레이어의 컨트롤에 제한이생김 > y축으로만 움직이게 되며, 상하를 제외한 움직임으로 사다리를 벗어날 수 없음
콜라이더와 바닥사이 플레이어가 자주 충돌하게되어 밀리거나 낑기는 현상을 방지하기위해 콜라이더를 물체보다 상단에 위치, 이로서 플레이어가 W를 눌러 상승하더라도 범위를 벗어나게되면 앞으로 이동하기 때문에 
콜라이더 사이에 끼지 않음

AddForce를 사용하게 되면 사다리에서 벗어나는 경우 대포처럼 날아가게 되는데 이를 velocity로 키보드에서 입력이 없는경우 0으로 지정해 주어 더 이상 위로 튀어나가지 않습니다.

### 3. 
레이캐스트를 활용한 포탑

바닥에 있는경우와 하늘에 떠있는 경우 플레이어를 감지하는 범위가 달라짐

>> 아직도 이해를 못해서 원인을 못찾음
>> 포탑이 두개인경우 코드가 겹쳐서인지? 플레이어를 찾는 로직이 겹쳐서? 작동이 되지 않는것 같음, 그러나 혼자인경우 작동되는 것을 확인
>> 가장 자료를 많이 참고한 기능이며 물리적인 움직임을 많이 공부 할 수 있었던 기능입니다.
>>
>> 완성이라고 생각했으나 프로젝트 마지막 테스트를 진행중 그렇지 않다는 것을 발견했습니다.
>> 결국 레이캐스트 작동 방식에서 플레이어 타겟이 Null이 되어 작동방식을 오버랩함수로 교체하였습니다.
>> 원형 범위를 감지하고, 특정 각도를 제한하여 해당 범위 밖으로 가면 감지되지 않습니다.
>> 개선이 많이 필요하다고 생각되며, 기능적인 부분만 구현해 놓았습니다.
>
### 4. 
폴가이즈라는 게임을 참고하여 장애물을 제작중에
움직이는 발판은 플레이어를 자식으로 두어 같이 움직이게 한다는 것은 알았으나 .. 플레이어의 움직임또한 제한되어 점프하지 않으면 움직임이 부자연 스러워집니다.

#### 마무리
프로젝트에서 점프대와 발사대가 존재하는데, 각 기능별로 색깔을 추가하여 알아보기 쉽게 만들었습니다.

점프대와 발사대로 장애물 기믹을 만들었지만 막상 발사대를 활용한 기능을 만드는데 수학적 계산이 필요하여, 일부 포기하게 되었습니다.
>>좌우로 통통 튕기며 앞으로 나아가는 기믹등..
2D와 다르게 3차원적인 물리력과 충돌을 생각해야 되서 상상과는 다르게 프로젝트가 진행되어 매우 아쉽습니다.
>>유연한 사고와, 진보하는 것이 중요하다고 느낀 회차였습니다.
