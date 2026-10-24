## Дата-центр РНД

rnd-array-close-first = Сначала закройте стойку.
rnd-array-locked = Стойка заблокирована.
rnd-array-verb-open = Открыть стойку
rnd-array-verb-close = Закрыть стойку
rnd-array-disk-broken = Накопитель дисциплины «{ $discipline }» вышел из строя!

rnd-array-slot-engineering = Слот инженерии
rnd-array-slot-medical = Слот медицины
rnd-array-slot-salvage = Слот утилизации
rnd-array-slot-science = Слот науки
rnd-array-slot-service = Слот сервиса
rnd-array-slot-shiparmory = Слот корабельного вооружения

rnd-array-ui-title = Дата-центр РНД
rnd-array-ui-status = { $lock }, { $rack }
rnd-array-ui-locked = Заблокирован
rnd-array-ui-unlocked = Разблокирован
rnd-array-ui-opened = стойка открыта
rnd-array-ui-closed = стойка закрыта
rnd-array-ui-info = Очки: { $points }. Подключено устройств: { $clients }
rnd-array-ui-open = Открыть стойку
rnd-array-ui-close = Закрыть стойку
rnd-array-ui-slots = Накопители:
rnd-array-ui-no-disk = Нет накопителя
rnd-array-ui-broken = Сломан
rnd-array-ui-condition = ~{ $condition }%
rnd-array-ui-technologies = Технологий: { $count }
rnd-array-ui-eject = Извлечь

rnd-disk-examine-blank = Назначение: [color=gray]нет, накопитель пуст[/color].
rnd-disk-examine-discipline = Дисциплина: [color=yellow]{ $discipline }[/color].
rnd-disk-examine-broken = [color=red]Накопитель сломан, данные утеряны.[/color]
rnd-disk-examine-condition = Состояние: ~{ $condition }%.
rnd-disk-examine-technologies = Записано технологий: { $count }.

## Сеть РНД

rnd-network-device-name = { $device } ({ $grid })
rnd-network-ui-online = В сети
rnd-network-ui-offline = Не в сети
rnd-network-ui-none = Нет
rnd-network-ui-disconnect = Отключить

rnd-relay-ui-title = РНД ретранслятор
rnd-relay-ui-level = Уровень: { $level }
rnd-relay-ui-capacity = Каналы: занято { $used } из { $capacity }, свободно { $free }
rnd-relay-ui-access-points = Подключённые точки доступа:

rnd-access-point-ui-title = Точка доступа РНД
rnd-access-point-ui-linked = Связь с дата-центром РНД есть
rnd-access-point-ui-unlinked = Нет связи с дата-центром РНД
rnd-access-point-ui-relay = Ретранслятор: { $relay }
rnd-access-point-ui-relays = Ретрансляторы:
rnd-access-point-ui-free = свободно { $free }/{ $capacity }
rnd-access-point-ui-connect = Подключить
rnd-access-point-relay-offline = Ретранслятор не в сети.
rnd-access-point-relay-full = У ретранслятора нет свободных каналов.

## Сущности

ent-HorizonRndArray = дата-центр РНД
    .desc = Серверная стойка с РНД накопителями, на которых хранятся все изученные технологии. Машины в других местах получают к ней доступ через РНД ретрансляторы.
ent-HorizonRndDisk = РНД накопитель
    .desc = Универсальный накопитель для дата-центра РНД. Получает дисциплину слота, в который его вставили.
ent-HorizonRndRelay = РНД ретранслятор (ур. 1)
    .desc = Связывает точки доступа РНД с дата-центром РНД. Каждая точка доступа занимает один канал. Каналов: 5.
ent-HorizonRndRelay2 = РНД ретранслятор (ур. 2)
    .desc = Связывает точки доступа РНД с дата-центром РНД. Каждая точка доступа занимает один канал. Каналов: 8.
ent-HorizonRndRelay3 = РНД ретранслятор (ур. 3)
    .desc = Связывает точки доступа РНД с дата-центром РНД. Каждая точка доступа занимает один канал. Каналов: 12.
ent-HorizonRndRelay4 = РНД ретранслятор (ур. 4)
    .desc = Связывает точки доступа РНД с дата-центром РНД. Каждая точка доступа занимает один канал. Каналов: 16.
ent-HorizonRndRelay5 = РНД ретранслятор (ур. 5)
    .desc = Связывает точки доступа РНД с дата-центром РНД. Каждая точка доступа занимает один канал. Каналов: 20.
ent-HorizonRndAccessPoint = точка доступа РНД
    .desc = Даёт исследовательским машинам корабля технологии дата-центра РНД. Подключите её к РНД ретранслятору.
ent-HorizonRndRelayMachineCircuitboard = машинная плата РНД ретранслятора (ур. 1)
    .desc = Печатная плата для РНД ретранслятора 1-го уровня.
ent-HorizonRndRelay2MachineCircuitboard = машинная плата РНД ретранслятора (ур. 2)
    .desc = Печатная плата для РНД ретранслятора 2-го уровня.
ent-HorizonRndRelay3MachineCircuitboard = машинная плата РНД ретранслятора (ур. 3)
    .desc = Печатная плата для РНД ретранслятора 3-го уровня.
ent-HorizonRndRelay4MachineCircuitboard = машинная плата РНД ретранслятора (ур. 4)
    .desc = Печатная плата для РНД ретранслятора 4-го уровня.
ent-HorizonRndRelay5MachineCircuitboard = машинная плата РНД ретранслятора (ур. 5)
    .desc = Печатная плата для РНД ретранслятора 5-го уровня.
ent-HorizonRndAccessPointMachineCircuitboard = машинная плата точки доступа РНД
    .desc = Печатная плата для точки доступа РНД.
