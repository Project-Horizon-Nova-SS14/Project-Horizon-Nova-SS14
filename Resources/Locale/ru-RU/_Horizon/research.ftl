## РНД Массив

rnd-array-close-first = Сначала закройте стойку.
rnd-array-locked = Стойка заблокирована.
rnd-array-verb-open = Открыть стойку
rnd-array-verb-close = Закрыть стойку
rnd-array-disk-broken = Диск дисциплины «{ $discipline }» вышел из строя!

rnd-array-slot-engineering = Слот инженерии
rnd-array-slot-medical = Слот медицины
rnd-array-slot-salvage = Слот утилизации
rnd-array-slot-science = Слот науки
rnd-array-slot-service = Слот сервиса
rnd-array-slot-shiparmory = Слот корабельного вооружения

rnd-array-ui-title = РНД Массив
rnd-array-ui-status = { $lock }, { $rack }
rnd-array-ui-locked = Заблокирован
rnd-array-ui-unlocked = Разблокирован
rnd-array-ui-opened = стойка открыта
rnd-array-ui-closed = стойка закрыта
rnd-array-ui-info = Очки: { $points }. Подключено устройств: { $clients }
rnd-array-ui-open = Открыть стойку
rnd-array-ui-close = Закрыть стойку
rnd-array-ui-slots = Диски:
rnd-array-ui-no-disk = Нет диска
rnd-array-ui-broken = Сломан
rnd-array-ui-condition = ~{ $condition }%
rnd-array-ui-technologies = Технологий: { $count }
rnd-array-ui-eject = Извлечь

rnd-disk-examine-discipline = Дисциплина: [color=yellow]{ $discipline }[/color].
rnd-disk-examine-broken = [color=red]Диск сломан, данные утеряны.[/color]
rnd-disk-examine-condition = Состояние: ~{ $condition }%.
rnd-disk-examine-technologies = Записано технологий: { $count }.

## Сеть РНД

rnd-network-device-name = { $device } ({ $grid })
rnd-network-ui-online = В сети
rnd-network-ui-offline = Не в сети
rnd-network-ui-none = Нет
rnd-network-ui-disconnect = Отключить

rnd-relay-ui-title = РНД Ретранслятор
rnd-relay-ui-capacity = Каналы: занято { $used } из { $capacity }, свободно { $free }
rnd-relay-ui-access-points = Подключённые точки доступа:

rnd-access-point-ui-title = Точка доступа РНД
rnd-access-point-ui-linked = Связь с РНД Массивом есть
rnd-access-point-ui-unlinked = Нет связи с РНД Массивом
rnd-access-point-ui-relay = Ретранслятор: { $relay }
rnd-access-point-ui-relays = Ретрансляторы:
rnd-access-point-ui-free = свободно { $free }/{ $capacity }
rnd-access-point-ui-connect = Подключить
rnd-access-point-relay-offline = Ретранслятор не в сети.
rnd-access-point-relay-full = У ретранслятора нет свободных каналов.

## Сущности

ent-HorizonRndArray = РНД Массив
    .desc = Серверная стойка с дисками, на которых хранятся все изученные технологии. Машины в других местах получают к ней доступ через РНД ретрансляторы.
ent-HorizonRndDiskEngineering = диск РНД Массива (инженерия)
    .desc = Диск для слота инженерии РНД Массива.
ent-HorizonRndDiskMedical = диск РНД Массива (медицина)
    .desc = Диск для слота медицины РНД Массива.
ent-HorizonRndDiskSalvage = диск РНД Массива (утилизация)
    .desc = Диск для слота утилизации РНД Массива.
ent-HorizonRndDiskScience = диск РНД Массива (наука)
    .desc = Диск для слота науки РНД Массива.
ent-HorizonRndDiskService = диск РНД Массива (сервис)
    .desc = Диск для слота сервиса РНД Массива.
ent-HorizonRndDiskShiparmory = диск РНД Массива (корабельное вооружение)
    .desc = Диск для слота корабельного вооружения РНД Массива.
ent-HorizonRndRelay = РНД ретранслятор
    .desc = Связывает точки доступа РНД с РНД Массивом. Каждая точка доступа занимает один канал.
ent-HorizonRndAccessPoint = точка доступа РНД
    .desc = Даёт исследовательским машинам корабля технологии РНД Массива. Подключите её к РНД ретранслятору.
ent-HorizonRndRelayMachineCircuitboard = машинная плата РНД ретранслятора
    .desc = Печатная плата для РНД ретранслятора.
ent-HorizonRndAccessPointMachineCircuitboard = машинная плата точки доступа РНД
    .desc = Печатная плата для точки доступа РНД.
